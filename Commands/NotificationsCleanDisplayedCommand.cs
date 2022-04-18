using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Injection;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands
{
    public sealed class NotificationsCleanDisplayedCommand : Command<bool>
    {
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute(bool focused)
        {
            if (!focused)
                NotificationsController.CleanDisplayedNotifications();
        }
    }
}