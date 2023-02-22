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
            RequestAuthorization(() =>
            {
                base.OnInitialize(status);
            });
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
            RequestAuthorization(() =>
            {
                OnAuthorizationComplete(GetAuthorizationStatus());
            });
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
                    yield return null;

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
         * Scheduling.
         */

        protected override void OnScheduleNotification(Notification notification)
        {
            var timeTrigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = new TimeSpan(0, 0, notification.TimeoutSeconds),
                Repeats = false
            };

            var iOSNotification = new iOSNotification
            {
                Identifier = notification.idString,

                Title = notification.title,
                Subtitle = notification.subTitle,
                Body = notification.text,

                ShowInForeground = notification.ShowInForeground,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Badge | PresentationOption.Sound,
                Badge = 1,
                CategoryIdentifier = "default_category",
                ThreadIdentifier = "default_thread",
                Trigger = timeTrigger
            };

            iOSNotificationCenter.ScheduleNotification(iOSNotification);
        }

        /*
         * Cancelling.
         */

        protected override void OnCancelScheduledNotification(Notification notification)
        {
            iOSNotificationCenter.RemoveScheduledNotification(notification.idString);
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
            if (Initialized && !Autorizing && !paused)
                TryUpdateAuthorizationStatus(GetAuthorizationStatus());
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            if (!Initialized || Autorizing || status != NotificationsAuthorizationStatus.Authorized) 
                return;
            
            if (!TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
                GetFirebaseToken(null);
            
            if (!TryGetToken(NotificationsTokenType.IOSDeviceToken, out _))
                OnRequestAuthorization();
        }
    }
}
#endif