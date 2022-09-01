using Build1.PostMVC.Core.Extensions.MVCS.Commands;
using Build1.PostMVC.Core.Extensions.MVCS.Events;
using Build1.PostMVC.Core.Extensions.MVCS.Injection;
using Build1.PostMVC.Extensions.MVCS.Events;

namespace Build1.PostMVC.UnityNotifications.Commands
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