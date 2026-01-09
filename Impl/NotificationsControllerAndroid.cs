#if UNITY_ANDROID || UNITY_EDITOR

using System;
using System.Collections;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.App.Modules.Coroutines;
using Unity.Notifications.Android;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerAndroid : NotificationsControllerBase
    {
        private const string     DefaultChannelId          = "main";
        private const string     DefaultChannelName        = "Main Channel";
        private const string     DefaultChannelDescription = "Main notifications channel";
        private const Importance DefaultChannelImportance  = Importance.High;

        [Inject] public ICoroutineProvider CoroutineProvider { get; set; }

        private Coroutine _coroutine;

        [PostConstruct]
        public void PostConstruct()
        {
            Dispatcher.AddListener(AppEvent.Pause, OnAppPause);
            Dispatcher.AddListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);
        }

        [PreDestroy]
        public void PreDestroy()
        {
            Dispatcher.RemoveListener(AppEvent.Pause, OnAppPause);
            Dispatcher.RemoveListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);
            
            CoroutineProvider.StopCoroutine(ref _coroutine);
        }
        
        /*
         * Initialization.
         */

        protected override void OnInitialize(NotificationsAuthorizationStatus status)
        {
            AndroidNotificationCenter.Initialize();
            
            if (status == NotificationsAuthorizationStatus.Authorized)
                RegisterNotificationChannel();
            
            base.OnInitialize(status);
        }

        private void RegisterNotificationChannel()
        {
            var channel = new AndroidNotificationChannel(DefaultChannelId, DefaultChannelName, DefaultChannelDescription, DefaultChannelImportance)
            {
                CanBypassDnd = false,
                CanShowBadge = true,
                EnableLights = true,
                EnableVibration = true,
                LockScreenVisibility = LockScreenVisibility.Public
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        /*
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return PermissionStatusToNotificationsAuthorizationStatus(AndroidNotificationCenter.UserPermissionToPost);
        }

        protected override void OnRequestAuthorization()
        {
            CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(), out _coroutine);
        }

        private IEnumerator RequestAuthorizationCoroutine()
        {
            Log.Debug("Request authorization...");

            var request = new PermissionRequest();
            while (request.Status == PermissionStatus.RequestPending)
                yield return null;

            Log.Debug(r => $"Request authorization complete: {r.Status}", request);
            
            _coroutine = null;

            var status = PermissionStatusToNotificationsAuthorizationStatus(request.Status);
            if (status == NotificationsAuthorizationStatus.Authorized)
                RegisterNotificationChannel();
            
            OnAuthorizationComplete(status);
        }
        
        /*
         * Native Settings.
         */

        public override void OpenNativeSettings()
        {
            try
            {
                using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                var packageName = currentActivityObject.Call<string>("getPackageName");

                using var uriClass = new AndroidJavaClass("android.net.Uri");
                using var uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
                using var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject);
                intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                currentActivityObject.Call("startActivity", intentObject);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
        }
        
        /*
         * Tokens.
         */

        protected override bool CheckFirebaseTokenLoadingAllowed()
        {
            return true;
        }
        
        /*
         * App badge.
         */

        public override void SetAppBadgeCounter(int number)
        {
            // Not implemented for Android.
        }

        /*
         * Scheduling.
         */

        protected override void OnScheduleNotification(Notification notification)
        {
            var androidNotification = new AndroidNotification
            {
                Title = notification.Title,
                Text = notification.Text
            };

            if (notification.AppBadgeCount >= 0)
                androidNotification.Number = notification.AppBadgeCount; 

            if (string.IsNullOrWhiteSpace(notification.AndroidGroupId))
                androidNotification.Group = notification.AndroidGroupId;

            if (notification.AndroidIconLarge != null)
                androidNotification.LargeIcon = notification.AndroidIconLarge;
            else if (notification.AndroidIconSmall != null)
                androidNotification.SmallIcon = notification.AndroidIconSmall;

            androidNotification.FireTime = DateTime.Now.AddSeconds(notification.TimeoutSeconds);

            AndroidNotificationCenter.SendNotificationWithExplicitID(androidNotification, DefaultChannelId, notification.id);
        }

        /*
         * Cancelling.
         */

        protected override void OnCancelScheduledNotification(Notification notification)
        {
            AndroidNotificationCenter.CancelScheduledNotification(notification.id);
        }

        protected override void OnCancelAllScheduledNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }

        /*
         * Cleaning.
         */

        protected override void OnCleanDisplayedNotifications()
        {
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
        }

        /*
         * Helpers.
         */

        private NotificationsAuthorizationStatus PermissionStatusToNotificationsAuthorizationStatus(PermissionStatus status)
        {
            return status switch
            {
                PermissionStatus.Allowed                    => NotificationsAuthorizationStatus.Authorized,
                PermissionStatus.Denied                     => NotificationsAuthorizationStatus.Denied,
                PermissionStatus.DeniedDontAskAgain         => NotificationsAuthorizationStatus.Denied,
                PermissionStatus.NotRequested               => NotificationsAuthorizationStatus.NotDetermined,
                PermissionStatus.RequestPending             => NotificationsAuthorizationStatus.NotDetermined,
                PermissionStatus.NotificationsBlockedForApp => NotificationsAuthorizationStatus.Denied,
                _                                           => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        /*
         * Event Handlers.
         */

        private void OnAppPause(bool paused)
        {
            if (Initialized && !Authorizing && !paused)
                TryUpdateAuthorizationStatus(GetAuthorizationStatus());
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            if (!Initialized || Authorizing || status != NotificationsAuthorizationStatus.Authorized) 
                return;
            
            if (!TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
                GetFCMToken();
        }
    }
}

#endif