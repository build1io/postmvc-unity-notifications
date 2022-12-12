using System;

namespace Build1.PostMVC.Unity.Notifications
{
    [Flags]
    public enum NotificationsSettings
    {
        RegisterForRemoteNotifications = 1 << 0,
        DelayAuthorization             = 1 << 1
    }
}