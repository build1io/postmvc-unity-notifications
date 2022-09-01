using System;
using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Injection;

namespace Build1.PostMVC.Unity.Notifications.Commands
{
    [Poolable]
    public sealed class NotificationsClearCommand : Command<NotificationState>
    {
        [Inject] public INotificationsController NotificationsController { get; set; }
        
        public override void Execute(NotificationState states)
        {
            switch (states)
            {
                case NotificationState.All:
                    NotificationsController.CleanDisplayedNotifications();
                    NotificationsController.CancelAllScheduledNotifications();
                    break;
                case NotificationState.Scheduled:
                    NotificationsController.CancelAllScheduledNotifications();
                    break;
                case NotificationState.Displayed:
                    NotificationsController.CleanDisplayedNotifications();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(states), states, null);
            }
        }
    }
}