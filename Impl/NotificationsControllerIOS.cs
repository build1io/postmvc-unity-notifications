#if UNITY_IOS || UNITY_EDITOR

using System;
using System.Collections;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.App.Modules.Coroutines;
using Build1.PostMVC.Unity.App.Modules.InternetReachability;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerIOS : NotificationsControllerBase
    {
        private const AuthorizationOption AuthorizationOptions = AuthorizationOption.Alert |
                                                                 AuthorizationOption.Badge |
                                                                 AuthorizationOption.Sound;

        [Inject] public ICoroutineProvider              CoroutineProvider              { get; set; }
        [Inject] public IInternetReachabilityController InternetReachabilityController { get; set; }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetSettingsURL();

        private Coroutine _coroutine;

        [PostConstruct]
        public void PostConstruct()
        {
            Dispatcher.AddListener(AppEvent.Pause, OnAppPause);
            Dispatcher.AddListener(AppEvent.Focus, OnAppFocus);
            Dispatcher.AddListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);
        }

        [PreDestroy]
        public void PreDestroy()
        {
            Dispatcher.RemoveListener(AppEvent.Pause, OnAppPause);
            Dispatcher.RemoveListener(AppEvent.Focus, OnAppFocus);
            Dispatcher.RemoveListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);

            CoroutineProvider.StopCoroutine(ref _coroutine);
        }

        /*
         * Initializing.
         */

        protected override void OnInitialize(NotificationsAuthorizationStatus status)
        {
            if (status != NotificationsAuthorizationStatus.Authorized)
            {
                base.OnInitialize(status);
                return;
            }

            // If user already authorized notifications, we request authorization anyway.
            // It runs silently and loads an Apple Push Notifications Token. It might be used by other components.
            RequestAuthorization(() => { base.OnInitialize(status); });
        }

        /*
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            var status = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            return AuthorizationStatusToNotificationsAuthorizationStatus(status);
        }

        protected override void OnRequestAuthorization()
        {
            RequestAuthorization(() => { OnAuthorizationComplete(GetAuthorizationStatus()); });
        }

        private void RequestAuthorization(Action onComplete)
        {
            if (!RegisterForRemoteNotifications)
            {
                RequestAuthorizationImpl(false, onComplete);
                return;
            }

            Log.Debug("Checking internet connection...");

            InternetReachabilityController.Check(reachable =>
            {
                Log.Debug(log =>
                {
                    log.Debug(reachable ? "Internet is reachable" : "Internet not reachable");
                    log.Debug("Requesting authorization...");
                });

                RequestAuthorizationImpl(reachable, onComplete);
            });
        }

        private void RequestAuthorizationImpl(bool registerForRemoteNotifications, Action onComplete)
        {
            CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(AuthorizationOptions, registerForRemoteNotifications, onComplete), out _coroutine);
        }

        private IEnumerator RequestAuthorizationCoroutine(AuthorizationOption authorizationOptions, bool registerForRemoteNotifications, Action onComplete)
        {
            Log.Debug((a, r) => $"RequestAuthorizationCoroutine: options: {a} remote: {r}", authorizationOptions, registerForRemoteNotifications);

            using (var request = new AuthorizationRequest(authorizationOptions, registerForRemoteNotifications))
            {
                while (!request.IsFinished)
                {
                    Log.Debug("Authorization request pending...");
                    yield return null;
                }

                _coroutine = null;

                Log.Debug("Authorization request complete");

                if (request.Granted)
                {
                    Log.Debug(t => $"Authorized. DeviceToken: \'{t}\'", request.DeviceToken);

                    AddToken(NotificationsTokenType.IOSDeviceToken, request.DeviceToken);
                }
                else if (request.Error != null)
                {
                    Log.Error(e => $"Authorization error: {e}", request.Error);
                }
                else
                {
                    Log.Debug("Not authorized. User denied notifications request.");
                }

                onComplete();
            }
        }

        /*
         * Native Settings.
         */

        public override void OpenNativeSettings()
        {
            var url = GetSettingsURL();

            Log.Debug(u => $"The settings url is: {u}", url);

            Application.OpenURL(url);
        }

        /*
         * Tokens.
         */

        protected override bool CheckFirebaseTokenLoadingAllowed()
        {
            // Firebase token loading allowed only if Apple token is loaded.
            return TryGetToken(NotificationsTokenType.IOSDeviceToken, out _);
        }

        /*
         * App badge.
         */

        public override void SetAppBadgeCounter(int number)
        {
            Log.Debug(a => $"SetAppBadgeCounter: {a}", number);

            iOSNotificationCenter.ApplicationBadge = number;
        }

        /*
         * Scheduling.
         */

        protected override void OnScheduleNotification(Notification notification)
        {
            var timeTrigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = new TimeSpan(0, 0, Math.Max(notification.TimeoutSeconds, 1)),
                Repeats = false
            };

            var iOSNotification = new iOSNotification
            {
                Identifier = notification.id.ToString(),

                Title = notification.Title,
                Subtitle = notification.SubTitle,
                Body = notification.Text,

                ShowInForeground = notification.ShowInForeground,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Badge | PresentationOption.Sound,
                Badge = notification.AppBadgeCount,
                CategoryIdentifier = "default_category",
                ThreadIdentifier = string.IsNullOrWhiteSpace(notification.IOSThreadId) ? "default_thread" : notification.IOSThreadId,
                Trigger = timeTrigger
            };

            if (notification.IOSSoundName != null)
                iOSNotification.SoundName = notification.IOSSoundName;
            else if (Settings.DefaultSoundName != null)
                iOSNotification.SoundName = Settings.DefaultSoundName;

            iOSNotificationCenter.ScheduleNotification(iOSNotification);
        }

        /*
         * Cancelling.
         */

        protected override void OnCancelScheduledNotification(Notification notification)
        {
            iOSNotificationCenter.RemoveScheduledNotification(notification.id.ToString());
        }

        protected override void OnCancelAllScheduledNotifications()
        {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        /*
         * Cleaning.
         */

        protected override void OnCleanDisplayedNotifications()
        {
            iOSNotificationCenter.ApplicationBadge = 0;
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }

        /*
         * Helpers.
         */

        private NotificationsAuthorizationStatus AuthorizationStatusToNotificationsAuthorizationStatus(AuthorizationStatus status)
        {
            return status switch
            {
                global::Unity.Notifications.iOS.AuthorizationStatus.NotDetermined => NotificationsAuthorizationStatus.NotDetermined,
                global::Unity.Notifications.iOS.AuthorizationStatus.Denied        => NotificationsAuthorizationStatus.Denied,
                global::Unity.Notifications.iOS.AuthorizationStatus.Authorized    => NotificationsAuthorizationStatus.Authorized,
                global::Unity.Notifications.iOS.AuthorizationStatus.Provisional   => NotificationsAuthorizationStatus.Authorized,
                global::Unity.Notifications.iOS.AuthorizationStatus.Ephemeral     => NotificationsAuthorizationStatus.Authorized,
                _                                                                 => throw new ArgumentOutOfRangeException()
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

        private void OnAppFocus(bool focused)
        {
            if (Initialized && !Authorizing && focused)
                TryUpdateAuthorizationStatus(GetAuthorizationStatus());
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            if (!Initialized || Authorizing || status != NotificationsAuthorizationStatus.Authorized)
                return;

            if (!TryGetToken(NotificationsTokenType.IOSDeviceToken, out _))
            {
                // To load APN token.
                RequestAuthorization(() =>
                {
                    if (!TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
                        GetFirebaseToken(null);
                });
            }
            else if (!TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
            {
                GetFirebaseToken(null);
            }
        }
    }
}
#endif