using Build1.PostMVC.Core.MVCS.Events;

namespace Build1.PostMVC.Unity.Notifications
{
    public static class NotificationsEvent
    {
        public static readonly Event                                   Initialized                = new(typeof(NotificationsEvent), nameof(Initialized));
        public static readonly Event                                   AuthorizationRequesting    = new(typeof(NotificationsEvent), nameof(AuthorizationRequesting));
        public static readonly Event<NotificationsAuthorizationStatus> AuthorizationStatusChanged = new(typeof(NotificationsEvent), nameof(AuthorizationStatusChanged));
        public static readonly Event                                   AuthorizationCanceled      = new(typeof(NotificationsEvent), nameof(AuthorizationCanceled));
        public static readonly Event<NotificationsTokenType, string>   TokenAdded                 = new(typeof(NotificationsEvent), nameof(TokenAdded));
    }
}