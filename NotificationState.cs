using System;

namespace Build1.PostMVC.Unity.Notifications
{
    [Flags]
    public enum NotificationState
    {
        All = Scheduled | Displayed,
        
        Scheduled = 1 << 0,
        Displayed = 1 << 1
    }
}