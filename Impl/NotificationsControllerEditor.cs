#if UNITY_EDITOR

using System;
using Build1.PostMVC.Core.MVCS.Injection;
using Build1.PostMVC.Unity.App.Modules.App;
using UnityEditor;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerEditor : NotificationsControllerBase
    {
        private const string AuthorizationSetPlayerPrefsKey = "PostMVC_NotificationsControllerEditor_AuthorizationSet";

        [PostConstruct]
        public void PostConstruct()
        {
            Dispatcher.AddListener(AppEvent.Pause, OnAppPause);
            Dispatcher.AddListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);
        }

        [PreDestroy]
        public void PreDestroy()
        {
            Dispatcher.RemoveListener(AppEvent.Pause, OnAppPause);
            Dispatcher.RemoveListener(NotificationsEvent.AuthorizationStatusChanged, OnAuthorizationStatusChanged);
        }
        
        /*
         * Initialization.
         */

        protected override void OnInitialize(NotificationsAuthorizationStatus status)
        {
            switch (status)
            {
                case NotificationsAuthorizationStatus.NotDetermined:

                    if (DelayAuthorization)
                        CompleteInitialization();
                    else
                        RequestAuthorization();
                    
                    break;
                
                case NotificationsAuthorizationStatus.Authorized:
                    GetFirebaseToken(CompleteInitialization);
                    break;

                case NotificationsAuthorizationStatus.Denied:
                    CompleteInitialization();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        
        /*
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return GetAuthorizationStatusStatic();
        }

        protected override void OnRequestAuthorization()
        {
            Log.Debug("Editor simulation. Showing authorization editor dialog...");

            var status = EditorUtility.DisplayDialog("Notifications", "Would you like to allow notifications?", "Allow", "Don't allow")
                             ? NotificationsAuthorizationStatus.Authorized
                             : NotificationsAuthorizationStatus.Denied;

            SetAuthorizationStatusStatic(status);
            
            TryUpdateAuthorizationStatus(status, Initialized);
            
            OnAuthorizationComplete();

            switch (status)
            {
                case NotificationsAuthorizationStatus.Authorized:
                    GetFirebaseToken(Initialized ? null : CompleteInitialization);
                    break;

                case NotificationsAuthorizationStatus.Denied:
                    if (!Initialized)
                        CompleteInitialization();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        /*
         * Scheduling.
         */

        protected override void OnScheduleNotification(Notification notification) { }

        /*
         * Cancelling.
         */

        protected override void OnCancelScheduledNotification(Notification notification) { }
        protected override void OnCancelAllScheduledNotifications()                      { }

        /*
         * Cleaning.
         */

        protected override void OnCleanDisplayedNotifications() { }

        /*
         * Event Handlers.
         */

        private void OnAppPause(bool paused)
        {
            if (!paused && Initialized && !Autorizing) 
                TryUpdateAuthorizationStatus(GetAuthorizationStatus(), true);
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            if (status == NotificationsAuthorizationStatus.Authorized && !TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
                GetFirebaseToken(null);
        }
        
        /*
         * Static.
         */

        public static NotificationsAuthorizationStatus GetAuthorizationStatusStatic()
        {
            if (PlayerPrefs.HasKey(AuthorizationSetPlayerPrefsKey))
                return (NotificationsAuthorizationStatus)PlayerPrefs.GetInt(AuthorizationSetPlayerPrefsKey);
            return NotificationsAuthorizationStatus.NotDetermined;
        }

        public static void SetAuthorizationStatusStatic(NotificationsAuthorizationStatus status)
        {
            PlayerPrefs.SetInt(AuthorizationSetPlayerPrefsKey, (int)status);
        }
        
        public static void ResetAuthorizationStatusStatic()
        {
            PlayerPrefs.DeleteKey(AuthorizationSetPlayerPrefsKey);
        }
    }
}

#endif