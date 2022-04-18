using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Events;
using Build1.PostMVC.Extensions.MVCS.Injection;
using Build1.PostMVC.Extensions.Unity.Modules.Logging;

namespace Build1.PostMVC.Unity.Modules.Notifications.Commands
{
    public sealed class NotificationsInitializeNoRemoteNotificationsCommand : Command
    {
        [Log(LogLevel.Warning)] public ILog                     Log                     { get; set; }
        [Inject]                public IEventDispatcher         Dispatcher              { get; set; }
        [Inject]                public INotificationsController NotificationsController { get; set; }

        public override void Execute()
        {
            if (NotificationsController.Initialized)
            {
                Log.Debug("Already initialized");
                return;
            }

            Log.Debug("Initializing...");

            Retain();

            Dispatcher.AddListenerOnce(NotificationsEvent.Initialized, OnComplete);

            NotificationsController.Initialize(false);
        }

        private void OnComplete()
        {
            Log.Debug("Done");

            Release();
        }
    }
}