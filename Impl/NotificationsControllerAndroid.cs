#if UNITY_ANDROID

using System;
using Build1.PostMVC.Core.Extensions.MVCS.Events;
using Build1.PostMVC.Core.Extensions.MVCS.Injection;
using Build1.PostMVC.UnityApp.Modules.Logging;
using Unity.Notifications.Android;

namespace Build1.PostMVC.UnityNotifications.Impl
{
    internal sealed class NotificationsControllerAndroid : INotificationsController
    {
        private const string     DefaultChannelId          = "main";
        private const string     DefaultChannelName        = "Main Channel";
        private const string     DefaultChannelDescription = "Main notifications channel";
        private const Importance DefaultChannelImportance  = Importance.High;
        private const string     DefaultIcon               = "main";

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
                Log.Warn("Already initialized.");
                return;
            }

            var channel = new AndroidNotificationChannel
            {
                Id = DefaultChannelId,
                Name = DefaultChannelName,
                Importance = DefaultChannelImportance,
                Description = DefaultChannelDescription,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

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
            // Always true for Android.
            return true;
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
                Log.Error("Notification not initialized.");
                return;
            }

            if (!Enabled)
            {
                Log.Debug("Notifications disabled.");
                return;
            }

            var androidNotification = new AndroidNotification();
            androidNotification.Title = notification.title;
            androidNotification.Text = notification.text;

            if (notification.largeIcon != null)
                androidNotification.LargeIcon = notification.largeIcon;
            else if (notification.smallIcon != null)
                androidNotification.SmallIcon = notification.smallIcon;
            else
                androidNotification.LargeIcon = DefaultIcon;

            androidNotification.FireTime = DateTime.Now.AddSeconds(notification.TimeoutSeconds);

            AndroidNotificationCenter.SendNotificationWithExplicitID(androidNotification, DefaultChannelId, notification.id);
        }
        
        /*
         * Cancelling.
         */

        public void CancelScheduledNotification(Notification notification)
        {
            Log.Debug(i => $"CancelScheduledNotification: {i}", notification.id);
            
            AndroidNotificationCenter.CancelScheduledNotification(notification.id);
        }
        
        public void CancelAllScheduledNotifications()
        {
            Log.Debug("CancelAllScheduledNotifications");
            
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }
        
        /*
         * Cleaning.
         */

        public void CleanDisplayedNotifications()
        {
            if (!Initialized)
            {
                Log.Error("Notification not initialized.");
                return;
            }

            if (!Enabled)
            {
                Log.Debug("Notifications disabled.");
                return;
            }

            AndroidNotificationCenter.CancelAllDisplayedNotifications();
        }
    }
}

#endif