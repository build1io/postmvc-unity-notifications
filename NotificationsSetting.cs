using System;

namespace Build1.PostMVC.Unity.Notifications
{
    [Flags]
    public enum NotificationsSetting
    {
        RequestAuthorizationOnInit     = 0,
        DelayAuthorization             = 1 << 0,
        RegisterForRemoteNotifications = 1 << 1
    }
}