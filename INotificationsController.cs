namespace Build1.PostMVC.Unity.Notifications
{
    public interface INotificationsController
    {
        NotificationsAuthorizationStatus AuthorizationStatus { get; }
        bool                             Initializing        { get; }
        bool                             Initialized         { get; }
        bool                             Enabled             { get; }

        void Initialize(NotificationsSettings settings);
        void RequestAuthorization();

        void SetEnabled(bool enabled);

        void ScheduleNotification(Notification notification);

        bool TryGetToken(NotificationsTokenType tokenType, out string token);

        void CancelScheduledNotification(Notification notification);
        void CancelAllScheduledNotifications();

        void CleanDisplayedNotifications();
    }
}