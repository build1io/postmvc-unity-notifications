using Build1.PostMVC.Core.Modules;
using Build1.PostMVC.Core.MVCS.Commands;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using Build1.PostMVC.Unity.Notifications.Commands;
using Build1.PostMVC.Unity.Notifications.Impl;

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
                InjectionBinder.Bind<INotificationsController>().To<NotificationsControllerEditor>().AsSingleton();
            #elif UNITY_ANDROID
                InjectionBinder.Bind<INotificationsController>().To<NotificationsControllerAndroid>().AsSingleton();
            #elif UNITY_IOS
                InjectionBinder.Bind<INotificationsController>().To<NotificationsControllerIOS>().AsSingleton();
            #endif

            CommandBinder.Bind(AppEvent.Pause)
                         .TriggerValue(false)
                         .To0<NotificationsClearAllCommand>();

            #if UNITY_ANDROID && !UNITY_EDITOR
            
            CommandBinder.Bind(AppEvent.Focus)
                         .TriggerValue(false)
                         .To0<NotificationsCleanDisplayedCommand>();
            
            #endif

            CommandBinder.Bind(NotificationsEvent.Initialized)
                         .To<NotificationsClearAllCommand>();
        }
    }
}