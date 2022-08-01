namespace Build1.PostMVC.Unity.Modules.Notifications
{
    public interface INotificationsController
    {
        bool Initialized { get; }
        
        void Initialize(bool registerForRemoteNotifications);
        void SetEnabled(bool enabled);
        bool TryGetAuthorized(out bool value);

        void ScheduleNotification(Notification notification);

        void CancelScheduledNotification(Notification notification);
        void CancelAllScheduledNotifications();

        void CleanDisplayedNotifications();

    }
}