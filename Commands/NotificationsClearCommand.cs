using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Injection;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands
{
    public sealed class NotificationsClearCommand : Command<bool>
    {
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute(bool paused)
        {
            if (paused)
                return;
            
            NotificationsController.CleanDisplayedNotifications();
            NotificationsController.CancelAllScheduledNotifications();
        }
    }
}