using System;
using System.Collections.Generic;
using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.Logging;
using Firebase.Extensions;
using Firebase.Messaging;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    public abstract class NotificationsControllerBase : INotificationsController
    {
        [Log(LogLevel.All)] public ILog             Log        { get; set; }
        [Inject]            public IEventDispatcher Dispatcher { get; set; }

        public bool                             Autorizing          { get; private set; }
        public NotificationsAuthorizationStatus AuthorizationStatus => _status;
        public bool                             Initializing        { get; protected set; }
        public bool                             Initialized         { get; protected set; }
        public bool                             Enabled             { get; protected set; }

        protected bool DelayAuthorization             => (_settings & NotificationsSettings.DelayAuthorization) == NotificationsSettings.DelayAuthorization;
        protected bool RegisterForRemoteNotifications => (_settings & NotificationsSettings.RegisterForRemoteNotifications) == NotificationsSettings.RegisterForRemoteNotifications;

        private NotificationsSettings                      _settings;
        private NotificationsAuthorizationStatus           _status;
        private Dictionary<NotificationsTokenType, string> _tokens;
        private List<Notification>                         _notificationsToSchedule;

        /*
         * Initialization.
         */

        public void Initialize(NotificationsSettings settings)
        {
            if (Initialized)
            {
                Log.Warn("Already initialized");
                return;
            }

            if (Initializing)
            {
                Log.Warn("Already initializing");
                return;
            }

            Log.Debug("Initializing...");

            Initializing = true;

            _settings = settings;
            _status = GetAuthorizationStatus();

            OnInitialize(_status);
        }

        protected abstract void OnInitialize(NotificationsAuthorizationStatus status);

        protected void CompleteInitialization()
        {
            if (Initialized)
            {
                Log.Error("Already initialized");
                return;
            }
                

            Log.Debug(s => $"Initialized: {s}", _status);

            Initialized = true;
            Initializing = false;
            Dispatcher.Dispatch(NotificationsEvent.Initialized);
        }

        /*
         * Authorization.
         */

        protected abstract NotificationsAuthorizationStatus GetAuthorizationStatus();

        protected void TryUpdateAuthorizationStatus(NotificationsAuthorizationStatus status, bool dispatchEvent)
        {
            if (_status == status)
                return;

            Log.Debug(s => $"Authorization status updated: {s}", status);

            _status = status;

            if (dispatchEvent)
                Dispatcher.Dispatch(NotificationsEvent.AuthorizationStatusChanged, status);
        }

        protected void RequestAuthorization()
        {
            if (Autorizing)
            {
                Log.Error("Authorization already in progress");
                return;
            }

            Autorizing = true;
            OnRequestAuthorization();
        }

        protected abstract void OnRequestAuthorization();

        protected void OnAuthorizationComplete()
        {
            Autorizing = false;

            if (_notificationsToSchedule != null)
            {
                foreach (var notification in _notificationsToSchedule)
                    ScheduleNotification(notification);
                _notificationsToSchedule = null;
            }
        }

        /*
         * Settings.
         */

        public void SetEnabled(bool enabled)
        {
            Log.Debug(e => $"Set enabled: {e}", enabled);

            Enabled = enabled;
        }

        /*
         * Tokens.
         */

        public bool TryGetToken(NotificationsTokenType tokenType, out string token)
        {
            if (_tokens != null)
                return _tokens.TryGetValue(tokenType, out token);
            token = null;
            return false;
        }

        internal void AddToken(NotificationsTokenType type, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            _tokens ??= new Dictionary<NotificationsTokenType, string>();
            _tokens[type] = token;

            Dispatcher.Dispatch(NotificationsEvent.TokenAdded, type, token);
        }

        protected void GetFirebaseToken(Action onComplete)
        {
            Log.Debug("Requesting Firebase notifications token...");

            FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
            {
                var token = task.Result;

                Log.Debug(t => $"Firebase token received: {t}", token);

                AddToken(NotificationsTokenType.FirebaseDeviceToken, token);

                onComplete?.Invoke();
            });
        }

        /*
         * Scheduling.
         */

        public void ScheduleNotification(Notification notification)
        {
            if (!Initialized)
            {
                Log.Error("Notifications not initialized");
                return;
            }

            if (!Enabled)
            {
                Log.Debug("Notifications disabled");
                return;
            }

            Log.Debug(n => $"Starting notification scheduling: {n}", notification);

            switch (_status)
            {
                case NotificationsAuthorizationStatus.NotDetermined:

                    _notificationsToSchedule ??= new List<Notification>();
                    _notificationsToSchedule.Add(notification);

                    RequestAuthorization();
                    break;
                case NotificationsAuthorizationStatus.Authorized:
                    OnScheduleNotification(notification);
                    Log.Debug(n => $"Notification scheduled: {n}", notification);
                    break;
                case NotificationsAuthorizationStatus.Denied:
                    Log.Debug("Notifications not authorized");
                    break;
            }
        }

        protected abstract void OnScheduleNotification(Notification notification);

        /*
         * Cancelling.
         */

        public void CancelScheduledNotification(Notification notification)
        {
            Log.Debug(i => $"Cancelling scheduled notification: {i}", notification.id);

            OnCancelScheduledNotification(notification);
        }

        protected abstract void OnCancelScheduledNotification(Notification notification);

        public void CancelAllScheduledNotifications()
        {
            Log.Debug("Cancelling all scheduled notifications");

            OnCancelAllScheduledNotifications();
        }

        protected abstract void OnCancelAllScheduledNotifications();

        /*
         * Cleaning.
         */

        public void CleanDisplayedNotifications()
        {
            Log.Debug("Cleaning displayed notifications");

            OnCleanDisplayedNotifications();
        }

        protected abstract void OnCleanDisplayedNotifications();
    }
}