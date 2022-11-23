#if UNITY_IOS

using System;
using System.Collections;
using System.Collections.Generic;
using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.App.Modules.Async;
using Build1.PostMVC.Unity.App.Modules.Coroutines;
using Build1.PostMVC.Unity.App.Modules.InternetReachability;
using Build1.PostMVC.Unity.App.Modules.Logging;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerIOS : INotificationsController
    {
        private const AuthorizationOption AuthorizationOptions = AuthorizationOption.Alert |
                                                                 AuthorizationOption.Badge |
                                                                 AuthorizationOption.Sound;

        [Log(LogLevel.Warning)] public ILog                            Log                            { get; set; }
        [Inject]                public IAsyncResolver                  AsyncResolver                  { get; set; }
        [Inject]                public IEventDispatcher                Dispatcher                     { get; set; }
        [Inject]                public ICoroutineProvider              CoroutineProvider              { get; set; }
        [Inject]                public IInternetReachabilityController InternetReachabilityController { get; set; }

        public bool Initializing => _coroutine != null;
        public bool Initialized  { get; private set; }
        public bool Enabled      { get; private set; }

        private bool Authorized => _status == AuthorizationStatus.Authorized &&
                                   ((_registerForRemoteNotifications && _deviceToken != null) || !_registerForRemoteNotifications);

        private Coroutine           _coroutine;
        private AuthorizationStatus _status;
        private bool                _registerForRemoteNotifications;
        private string              _deviceToken;
        private List<Notification>  _notificationToSchedule;
        private int                 _callId;

        [PostConstruct]
        public void PostConstruct()
        {
            Dispatcher.AddListener(AppEvent.Pause, OnAppPause);
        }
        
        [PreDestroy]
        public void PreDestroy()
        {
            Dispatcher.RemoveListener(AppEvent.Pause, OnAppPause);
            CoroutineProvider.StopCoroutine(ref _coroutine);
            AsyncResolver.CancelCall(ref _callId);
        }

        /*
         * Initializing.
         */

        public void Initialize(bool registerForRemoteNotifications)
        {
            if (Initializing)
            {
                Log.Warn("Already initializing");
                return;
            }

            if (Initialized)
            {
                Log.Warn("Already initialized");
                return;
            }

            _status = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            _registerForRemoteNotifications = registerForRemoteNotifications;

            if (_status == AuthorizationStatus.Authorized && _registerForRemoteNotifications)
            {
                RequestAuthorization();
                return;
            }

            Initialized = true;
            Dispatcher.Dispatch(NotificationsEvent.Initialized);
        }

        private void RequestAuthorization()
        {
            Log.Debug("Checking internet...");

            if (!_registerForRemoteNotifications)
            {
                CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(AuthorizationOptions, false), out _coroutine);
                return;
            }

            InternetReachabilityController.Check(reachable =>
            {
                Log.Debug(log =>
                {
                    log.Debug(reachable ? "Internet is reachable" : "Internet not reachable");
                    log.Debug("Requesting authorization...");
                });

                CoroutineProvider.StartCoroutine(RequestAuthorizationCoroutine(AuthorizationOptions, reachable), out _coroutine);
            });
        }

        private IEnumerator RequestAuthorizationCoroutine(AuthorizationOption authorizationOptions, bool registerForRemoteNotifications)
        {
            Log.Debug((a, r) => $"RequestAuthorizationCoroutine: options: {a} remote: {r}", authorizationOptions, registerForRemoteNotifications);

            using (var request = new AuthorizationRequest(authorizationOptions, registerForRemoteNotifications))
            {
                while (!request.IsFinished)
                    yield return null;

                _coroutine = null;
                OnAuthorizationRequestComplete(request);
            }
        }

        private void OnAuthorizationRequestComplete(AuthorizationRequest request)
        {
            AuthorizationStatus status;
            
            if (request.Granted)
            {
                Log.Debug(t => $"Authorized. DeviceToken: {t}", request.DeviceToken);

                status = AuthorizationStatus.Authorized;
                _deviceToken = request.DeviceToken;

                if (_notificationToSchedule != null)
                {
                    // Delay needed as notifications scheduled right away don't work.
                    _callId = AsyncResolver.DelayedCall(() =>
                    {
                        // If any notifications were requested before authorization, they must be scheduled after it.
                        foreach (var notification in _notificationToSchedule)
                            ScheduleNotificationImpl(notification);

                        // Cleaning notification.
                        _notificationToSchedule = null;
                    }, 0.1F);
                }
            }
            else if (request.Error != null)
            {
                Log.Error(e => $"Authorization error: {e}", request.Error);

                status = AuthorizationStatus.Denied;
            }
            else
            {
                Log.Debug("Not authorized. User denied notifications request.");

                status = AuthorizationStatus.Denied;
            }

            if (!Initialized)
            {
                Initialized = true;
                Dispatcher.Dispatch(NotificationsEvent.Initialized);    
            }

            TryUpdateStatus(status);
        }

        /*
         * Status.
         */

        public NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            var status = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            TryUpdateStatus(status);
            return AuthorizationStatusToNotificationsAuthorizationStatus(status);
        }

        public bool CheckAuthorized()       { return GetAuthorizationStatus() == NotificationsAuthorizationStatus.Authorized; }
        public bool CheckAuthorizationSet() { return GetAuthorizationStatus() != NotificationsAuthorizationStatus.NotDetermined; }

        private bool TryUpdateStatus(AuthorizationStatus status)
        {
            if (_status == status)
                return false;
            
            _status = status;
            
            var notificationsStatus = AuthorizationStatusToNotificationsAuthorizationStatus(status);
            Dispatcher.Dispatch(NotificationsEvent.AuthorizationStatusChanged, notificationsStatus);

            return true;
        }
        
        private NotificationsAuthorizationStatus AuthorizationStatusToNotificationsAuthorizationStatus(AuthorizationStatus status)
        {
            return status switch
            {
                AuthorizationStatus.NotDetermined => NotificationsAuthorizationStatus.NotDetermined,
                AuthorizationStatus.Denied        => NotificationsAuthorizationStatus.Denied,
                AuthorizationStatus.Authorized    => NotificationsAuthorizationStatus.Authorized,
                _                                 => throw new ArgumentOutOfRangeException()
            };
        }
        
        /*
         * Enabling.
         */
        
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        /*
         * Scheduling.
         */

        public void ScheduleNotification(Notification notification)
        {
            Log.Debug("ScheduleNotification");

            if (!Initialized)
            {
                Log.Warn("Notifications not initialized");
                return;
            }

            if (!Enabled)
            {
                Log.Debug("Notifications disabled");
                return;
            }

            if (_status == AuthorizationStatus.NotDetermined)
            {
                Log.Debug("Notifications state not determined");

                // Adding notification to the list so it'll be scheduled after authorization request if user will grant access.
                _notificationToSchedule ??= new List<Notification>();
                _notificationToSchedule.Add(notification);

                RequestAuthorization();
                return;
            }

            ScheduleNotificationImpl(notification);
        }

        private void ScheduleNotificationImpl(Notification notification)
        {
            Log.Debug("ScheduleNotificationImpl");

            if (!Enabled)
            {
                Log.Debug("Notifications disabled.");
                return;
            }

            if (!Authorized)
            {
                Log.Warn("Notifications not authorized.");
                return;
            }

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

        public void CancelScheduledNotification(Notification notification)
        {
            Log.Debug(i => $"CancelScheduledNotification: {i}", notification.id);

            iOSNotificationCenter.RemoveScheduledNotification(notification.idString);
        }

        public void CancelAllScheduledNotifications()
        {
            Log.Debug("CancelAllScheduledNotifications");

            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        /*
         * Cleaning.
         */

        public void CleanDisplayedNotifications()
        {
            Log.Debug("CleanDisplayedNotifications");

            iOSNotificationCenter.ApplicationBadge = 0;
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }
        
        /*
         * Event Handlers.
         */

        private void OnAppPause(bool paused)
        {
            if (Initialized && !paused)
                GetAuthorizationStatus();
        }
    }
}
#endif