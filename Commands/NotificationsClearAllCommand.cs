using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Injection;

namespace Build1.PostMVC.Unity.Notifications.Commands
{
    [Poolable]
    public sealed class NotificationsClearAllCommand : Command
    {
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute()
        {
            NotificationsController.CleanDisplayedNotifications();
            NotificationsController.CancelAllScheduledNotifications();
        }
    }
}