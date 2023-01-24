using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Events;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.Logging;

namespace Build1.PostMVC.Unity.Notifications.Commands
{
    public sealed class NotificationsInitializeCommand : Command<NotificationsSettings>
    {
        [Log(LogLevel.Warning)] public ILog                     Log                     { get; set; }
        [Inject]                public IEventDispatcher         Dispatcher              { get; set; }
        [Inject]                public INotificationsController NotificationsController { get; set; }

        public override void Execute(NotificationsSettings settings)
        {
            if (NotificationsController.Initialized)
            {
                Log.Warn("Notifications already initialized");
                return;
            }

            Retain();

            Dispatcher.AddListenerOnce(NotificationsEvent.Initialized, Release);

            NotificationsController.Initialize(settings);
        }
    }
}