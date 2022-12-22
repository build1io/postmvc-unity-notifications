#if UNITY_ANDROID

using System;
using Unity.Notifications.Android;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerAndroid : NotificationsControllerBase
    {
        private const string     DefaultChannelId          = "main";
        private const string     DefaultChannelName        = "Main Channel";
        private const string     DefaultChannelDescription = "Main notifications channel";
        private const Importance DefaultChannelImportance  = Importance.High;
        private const string     DefaultIcon               = "main";

        /*
         * Initialization.
         */

        protected override void OnInitialize()
        {
            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
            {
                Id = DefaultChannelId,
                Name = DefaultChannelName,
                Importance = DefaultChannelImportance,
                Description = DefaultChannelDescription,
            });
        }

        /*
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return NotificationsAuthorizationStatus.Authorized;
        }

        protected override void RequestAuthorization(Notification notification)
        {
            CompleteAuthorization(NotificationsAuthorizationStatus.Authorized, notification);
        }

        /*
         * Scheduling.
         */

        protected override void OnScheduleNotification(Notification notification)
        {
            var androidNotification = new AndroidNotification
            {
                Title = notification.title,
                Text = notification.text
            };

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
    }
}

#endif