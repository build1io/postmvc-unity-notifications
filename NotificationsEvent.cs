using Build1.PostMVC.Core.MVCS.Events;

namespace Build1.PostMVC.Unity.Notifications
{
    public static class NotificationsEvent
    {
        public static readonly Event                                   Initialized                = new();
        public static readonly Event<NotificationsAuthorizationStatus> AuthorizationStatusChanged = new();
    }
}