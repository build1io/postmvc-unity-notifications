using Build1.PostMVC.Core.Extensions.MVCS.Commands;
using Build1.PostMVC.Core.Extensions.MVCS.Injection;

namespace Build1.PostMVC.UnityNotifications.Commands
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