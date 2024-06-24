using Build1.PostMVC.Core.Modules;
using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.Notifications.Commands;

namespace Build1.PostMVC.Unity.Notifications
{
    public sealed class NotificationsModule : Module
    {
        [Inject] public IInjectionBinder InjectionBinder { get; set; }
        [Inject] public ICommandBinder   CommandBinder   { get; set; }

        [PostConstruct]
        public void PostConstruct()
        {
            #if UNITY_EDITOR
                InjectionBinder.Bind<INotificationsController, Impl.NotificationsControllerEditor>();
            #elif UNITY_ANDROID
                InjectionBinder.Bind<INotificationsController, Impl.NotificationsControllerAndroid>();
            #elif UNITY_IOS
                InjectionBinder.Bind<INotificationsController, Impl.NotificationsControllerIOS>();
            #endif
            
            CommandBinder.Bind(AppEvent.Pause)
                         .TriggerCondition(false)
                         .To1<NotificationsClearCommand, NotificationState>(NotificationState.All);

            #if UNITY_ANDROID && !UNITY_EDITOR
            
            CommandBinder.Bind(AppEvent.Focus)
                         .TriggerCondition(false)
                         .To1<NotificationsClearCommand, NotificationState>(NotificationState.Displayed);
            
            #endif

            CommandBinder.Bind(NotificationsEvent.Initialized)
                         .To1<NotificationsClearCommand, NotificationState>(NotificationState.All);
        }
    }
}