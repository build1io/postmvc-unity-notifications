using Build1.PostMVC.Extensions.MVCS.Commands;
using Build1.PostMVC.Extensions.MVCS.Injection;
using Build1.PostMVC.Extensions.Unity.Modules.App;
using Build1.PostMVC.Modules;
using Build1.PostMVC.Unity.Modules.Notifications.Commands;

namespace Build1.PostMVC.Unity.Modules.Notifications
{
    public sealed class NotificationsModule : Module
    {
        [Inject] public IInjectionBinder InjectionBinder { get; set; }
        [Inject] public ICommandBinder   CommandBinder   { get; set; }

        [PostConstruct]
        public void PostConstruct()
        {
            #if UNITY_EDITOR
                InjectionBinder.Bind<INotificationsController>().To<Impl.NotificationsControllerEditor>().AsSingleton();
            #elif UNITY_ANDROID
                InjectionBinder.Bind<INotificationsController>().To<Impl.NotificationsControllerAndroid>().AsSingleton();
            #elif UNITY_IOS
                InjectionBinder.Bind<INotificationsController>().To<Impl.NotificationsControllerIOS>().AsSingleton();
            #endif

            CommandBinder.Bind(AppEvent.Pause).To<NotificationsClearCommand>();
            
            #if UNITY_ANDROID && !UNITY_EDITOR
                CommandBinder.Bind(AppEvent.Focus).To<NotificationsCleanDisplayedCommand>();
            #endif
            
            CommandBinder.Bind(NotificationsEvent.Initialized).To<NotificationsInitializedCommand>();
        }
    }
}