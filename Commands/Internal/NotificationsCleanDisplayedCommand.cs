#if UNITY_ANDROID

using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Injection;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands.Internal
{
    internal sealed class NotificationsCleanDisplayedCommand : Command<bool>
    {
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute(bool focused)
        {
            if (!focused)
                NotificationsController.CleanDisplayedNotifications();
        }
    }
}

#endif