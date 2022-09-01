#if UNITY_EDITOR

using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.Logging;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerEditor : INotificationsController
    {
        private string AuthorizationSetPlayerPrefsKey = "PostMVC_NotificationsControllerEditor_AuthorizationSet";
        
        [Log(LogLevel.Warning)] public ILog             Log        { get; set; }
        [Inject]                public IEventDispatcher Dispatcher { get; set; }

        public bool Initialized { get; private set; }
        public bool Enabled     { get; private set; }

        /*
         * Initialization.
         */

        public void Initialize(bool registerForRemoteNotifications)
        {
            if (Initialized)
            {
                Log.Warn("Already initialized");
                return;
            }

            Log.Debug("Initialized");

            Initialized = true;
            Dispatcher.Dispatch(NotificationsEvent.Initialized);
        }

        /*
         * Public.
         */

        public NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return NotificationsAuthorizationStatus.Authorized;
        }

        public bool CheckAuthorizationSet()
        {
            if (!PlayerPrefs.HasKey(AuthorizationSetPlayerPrefsKey))
                return false;
            
            var value = (NotificationsAuthorizationStatus)PlayerPrefs.GetInt(AuthorizationSetPlayerPrefsKey);
            return value != NotificationsAuthorizationStatus.NotDetermined; 
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        /*
         * Scheduling.
         */

        public void ScheduleNotification(Notification notification)
        {
            if (!Initialized)
            {
                Log.Warn("Notification not initialized.");
                return;
            }

            if (!Enabled)
            {
                Log.Debug("Notifications disabled.");
                return;
            }

            if (!PlayerPrefs.HasKey(AuthorizationSetPlayerPrefsKey))
                PlayerPrefs.SetInt(AuthorizationSetPlayerPrefsKey, (int)NotificationsAuthorizationStatus.Authorized);
            
            Log.Debug(n => $"Scheduling notification: {n}", notification);
        }

        /*
         * Cancelling.
         */

        public void CancelScheduledNotification(string id)
        {
            Log.Debug(i => $"Cancelling scheduled notification: {i}", id);
        }

        public void CancelScheduledNotification(Notification notification)
        {
            Log.Debug(i => $"Cancelling scheduled notification: {i}", notification.id);
        }

        public void CancelAllScheduledNotifications()
        {
            Log.Debug("Cancelling scheduled notifications");
        }

        /*
         * Cleaning.
         */

        public void CleanDisplayedNotifications()
        {
            Log.Debug("Cleaning displayed notifications");
        }
    }
}

#endif