using System;

namespace Build1.PostMVC.Unity.Notifications
{
    [Flags]
    public enum NotificationsSettings
    {
        RequestAuthorization           = 0,
        DelayAuthorization             = 1 << 0,
        RegisterForRemoteNotifications = 1 << 1
    }
}