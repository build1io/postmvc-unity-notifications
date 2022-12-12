#if UNITY_IOS

using System;
using System.Collections;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.App.Modules.Async;
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

        [Inject] public IAsyncResolver                  AsyncResolver                  { get; set; }
        [Inject] public ICoroutineProvider              CoroutineProvider              { get; set; }
        [Inject] public IInternetReachabilityController InternetReachabilityController { get; set; }
        
        protected override bool RemoteNotificationsAuthorizationRequired => true;

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
         * Authorization.
         */
        
        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            var status = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            return AuthorizationStatusToNotificationsAuthorizationStatus(status);
        }
        
        /*
         * TODO: multiple authorization attemps
         */

        protected override void RequestAuthorization(Notification notification)
        {
            if (!RegisterForRemoteNotifications)
            {
                CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(AuthorizationOptions, false, notification), out _coroutine);
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

                CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(AuthorizationOptions, reachable, notification), out _coroutine);
            });
        }

        private IEnumerator RequestAuthorizationCoroutine(AuthorizationOption authorizationOptions, bool registerForRemoteNotifications, Notification notification)
        {
            Log.Debug((a, r) => $"RequestAuthorizationCoroutine: options: {a} remote: {r}", authorizationOptions, registerForRemoteNotifications);

            using (var request = new AuthorizationRequest(authorizationOptions, registerForRemoteNotifications))
            {
                while (!request.IsFinished)
                    yield return null;

                _coroutine = null;
                OnAuthorizationRequestComplete(request, notification);
            }
        }

        private void OnAuthorizationRequestComplete(AuthorizationRequest request, Notification notification)
        {
            AuthorizationStatus status;

            if (request.Granted)
            {
                Log.Debug(t => $"Authorized. DeviceToken: {t}", request.DeviceToken);

                status = global::Unity.Notifications.iOS.AuthorizationStatus.Authorized;
                
                AddToken(NotificationsTokenType.IOSDeviceToken, request.DeviceToken);
            }
            else if (request.Error != null)
            {
                Log.Error(e => $"Authorization error: {e}", request.Error);

                status = global::Unity.Notifications.iOS.AuthorizationStatus.Denied;
            }
            else
            {
                Log.Debug("Not authorized. User denied notifications request.");

                status = global::Unity.Notifications.iOS.AuthorizationStatus.Denied;
            }

            CompleteAuthorization(AuthorizationStatusToNotificationsAuthorizationStatus(status), notification);
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
                _                                                                 => throw new ArgumentOutOfRangeException()
            };
        }

        /*
         * Event Handlers.
         */

        private void OnAppPause(bool paused)
        {
            if (!Initialized || paused) 
                return;
            
            AsyncResolver.Resolve(() =>
            {
                UpdateAuthorizationStatus(GetAuthorizationStatus());
            });
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            // This ust handle the case when user opens the app with disabled notifications in iOS device settings, then enables notifications and returns to the app.
            // Tokens must be requested 
            if (status == NotificationsAuthorizationStatus.Authorized && !TryGetToken(NotificationsTokenType.IOSDeviceToken, out _))
                RequestAuthorization(null);
        }
    }
}
#endif