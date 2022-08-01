using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Events;
using Build1.PostMVC.Extensions.MVCS.Injection;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands
{
    public sealed class NotificationsInitializeCommand : Command<bool>
    {
        [Inject] public IEventDispatcher         Dispatcher              { get; set; }
        [Inject] public INotificationsController NotificationsController { get; set; }

        public override void Execute(bool registerForRemoteNotifications)
        {
            if (NotificationsController.Initialized)
                return;

            Retain();

            Dispatcher.AddListenerOnce(NotificationsEvent.Initialized, Release);

            NotificationsController.Initialize(registerForRemoteNotifications);
        }
    }
}