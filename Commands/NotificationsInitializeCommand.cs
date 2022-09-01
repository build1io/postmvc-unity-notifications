using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;

namespace Build1.PostMVC.Unity.Notifications.Commands
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