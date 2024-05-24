using System;
using System.Collections.Generic;
using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.Logging;
using Firebase.Extensions;
using Firebase.Messaging;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal abstract class NotificationsControllerBase : INotificationsController
    {
        [Log(LogLevel.Warning)] public ILog             Log        { get; set; }
        [Inject]                public IEventDispatcher Dispatcher { get; set; }

        public bool                             Autorizing          { get; private set; }
        public NotificationsAuthorizationStatus AuthorizationStatus => _status;
        public bool                             Initializing        { get; protected set; }
        public bool                             Initialized         { get; protected set; }
        public bool                             Enabled             { get; protected set; } = true; // Enabled by default to make notifications work after initialization.

        protected NotificationsSettings Settings                       { get; private set; }
        protected bool                  DelayAuthorization             { get; private set; }
        protected bool                  RegisterForRemoteNotifications { get; private set; }

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

            Settings = settings;
            DelayAuthorization = (settings.Settings & NotificationsSetting.DelayAuthorization) == NotificationsSetting.DelayAuthorization;
            RegisterForRemoteNotifications = (settings.Settings & NotificationsSetting.RegisterForRemoteNotifications) == NotificationsSetting.RegisterForRemoteNotifications;
            
            _status = GetAuthorizationStatus();

            Log.Debug(s => $"Status: {s}", _status);

            OnInitialize(_status);
        }

        protected virtual void OnInitialize(NotificationsAuthorizationStatus status)
        {
            switch (status)
            {
                case NotificationsAuthorizationStatus.NotDetermined:

                    if (DelayAuthorization)
                        CompleteInitialization();
                    else
                        RequestAuthorization();

                    break;

                case NotificationsAuthorizationStatus.Authorized:
                    GetFirebaseToken(CompleteInitialization);
                    break;

                case NotificationsAuthorizationStatus.Denied:
                    CompleteInitialization();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        protected void CompleteInitialization()
        {
            if (Initialized)
            {
                Log.Error("Already initialized");
                return;
            }

            Log.Debug(s => $"Initialized. Status: {s}", _status);

            Initialized = true;
            Initializing = false;
            Dispatcher.Dispatch(NotificationsEvent.Initialized);
        }

        /*
         * Authorization.
         */

        protected abstract NotificationsAuthorizationStatus GetAuthorizationStatus();

        protected bool TryUpdateAuthorizationStatus(NotificationsAuthorizationStatus status)
        {
            if (_status == status)
                return false;

            Log.Debug(s => $"Authorization status updated: {s}", status);

            _status = status;
            Dispatcher.Dispatch(NotificationsEvent.AuthorizationStatusChanged, status);
            return true;
        }

        public void RequestAuthorization()
        {
            if (AuthorizationStatus != NotificationsAuthorizationStatus.NotDetermined)
            {
                Log.Error("Authorization request attempt while current status is determined");
                return;
            }

            if (Autorizing)
            {
                Log.Error("Authorization already in progress");
                return;
            }

            Autorizing = true;

            Dispatcher.Dispatch(NotificationsEvent.AuthorizationRequesting);

            OnRequestAuthorization();
        }

        protected abstract void OnRequestAuthorization();

        protected void OnAuthorizationComplete(NotificationsAuthorizationStatus status)
        {
            if (!Autorizing)
                return;

            Log.Debug(s => $"Authorization complete: {s}", status);

            if (!TryUpdateAuthorizationStatus(status))
                Dispatcher.Dispatch(NotificationsEvent.AuthorizationCanceled);

            Autorizing = false;

            if (_notificationsToSchedule != null)
            {
                // If app is not authorized, we don't bother trying to send notifications.
                if (status == NotificationsAuthorizationStatus.Authorized)
                {
                    foreach (var notification in _notificationsToSchedule)
                        ScheduleNotification(notification);
                }

                _notificationsToSchedule = null;
            }

            switch (status)
            {
                case NotificationsAuthorizationStatus.Authorized:
                    GetFirebaseToken(Initialized ? null : CompleteInitialization);
                    break;

                case NotificationsAuthorizationStatus.Denied:
                case NotificationsAuthorizationStatus.NotDetermined:
                    if (!Initialized)
                        CompleteInitialization();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
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
         * App badge.
         */

        public abstract void SetAppBadgeCounter(int number);

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

            Log.Debug((y, t) => $"Token received. Type: {y} Token: {t}", type, token);

            Dispatcher.Dispatch(NotificationsEvent.TokenAdded, type, token);
        }

        protected abstract bool CheckFirebaseTokenLoadingAllowed();

        protected void GetFirebaseToken(Action onComplete)
        {
            if (!CheckFirebaseTokenLoadingAllowed())
            {
                Log.Warn("Firebase token loading not allowed");
                onComplete?.Invoke();
                return;
            }

            Log.Debug("Requesting Firebase notifications token...");

            FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
            {
                var token = task.Result;

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