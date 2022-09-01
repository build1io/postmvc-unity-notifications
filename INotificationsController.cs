namespace Build1.PostMVC.UnityNotifications
{
    public interface INotificationsController
    {
        bool Initialized { get; }
        bool Enabled     { get; }

        void                             Initialize(bool registerForRemoteNotifications);
        NotificationsAuthorizationStatus GetAuthorizationStatus();
        bool                             CheckAuthorizationSet();
        void                             SetEnabled(bool enabled);

        void ScheduleNotification(Notification notification);

        void CancelScheduledNotification(Notification notification);
        void CancelAllScheduledNotifications();

        void CleanDisplayedNotifications();
    }
}