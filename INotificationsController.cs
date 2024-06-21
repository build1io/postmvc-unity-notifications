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
        void OpenNativeSettings();

        void SetEnabled(bool enabled);
        void SetAppBadgeCounter(int number);

        void ScheduleNotification(Notification notification);

        bool TryGetToken(NotificationsTokenType tokenType, out string token);

        void CancelScheduledNotification(Notification notification);
        void CancelAllScheduledNotifications();

        void CleanDisplayedNotifications();
    }
}