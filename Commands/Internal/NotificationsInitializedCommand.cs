using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Injection;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands.Internal
{
    internal sealed class NotificationsInitializedCommand : Command
    {
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute()
        {
            NotificationsController.CancelAllScheduledNotifications();
            NotificationsController.CleanDisplayedNotifications();
        }
    }
}