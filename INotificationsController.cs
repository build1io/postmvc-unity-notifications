namespace Build1.PostMVC.Unity.Modules.Notifications
{
    public interface INotificationsController
    {
        bool Initialized { get; }
        bool Enabled     { get; }

        void                             Initialize(bool registerForRemoteNotifications);
        NotificationsAuthorizationStatus GetAuthorizationStatus();
        void                             SetEnabled(bool enabled);

        void ScheduleNotification(Notification notification);

        void CancelScheduledNotification(Notification notification);
        void CancelAllScheduledNotifications();

        void CleanDisplayedNotifications();
    }
}