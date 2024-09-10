namespace Build1.PostMVC.Unity.Notifications
{
    public interface INotificationsController
    {
        bool                             Initializing        { get; }
        bool                             Initialized         { get; }
        bool                             Enabled             { get; }
        bool                             Authorizing         { get; }
        bool                             Authorized          { get; }
        NotificationsAuthorizationStatus AuthorizationStatus { get; }

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