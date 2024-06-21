#if UNITY_EDITOR

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
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return GetAuthorizationStatusStatic();
        }

        protected override void OnRequestAuthorization()
        {
            Log.Debug("Editor simulation. Showing authorization editor dialog...");
            
            var option = EditorUtility.DisplayDialogComplex("Notifications",
                                                            "Would you like to allow notifications?",
                                                            "Allow",
                                                            "Cancel",
                                                            "Don't allow");

            var status = option switch
            {
                0 => NotificationsAuthorizationStatus.Authorized,
                2 => NotificationsAuthorizationStatus.Denied,
                _ => NotificationsAuthorizationStatus.NotDetermined
            };

            SetAuthorizationStatusStatic(status);
            OnAuthorizationComplete(status);
        }
        
        /*
         * Native Settings.
         */

        public override void OpenNativeSettings()
        {
            Log.Debug("Editor simulation. Showing authorization editor dialog...");
            
            EditorUtility.DisplayDialog("Notifications", "Imagine this is a notification settings window.", "Close");
        }

        /*
         * Tokens.
         */

        protected override bool CheckFirebaseTokenLoadingAllowed()
        {
            return true;
        }
        
        /*
         * App badge.
         */

        public override void SetAppBadgeCounter(int number)
        {
            Log.Debug(a => $"SetAppBadgeCounter: {a}", number);
            
            // Not implemented for Editor.
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
            if (Initialized && !Autorizing && !paused)
                TryUpdateAuthorizationStatus(GetAuthorizationStatus());
        }

        private void OnAuthorizationStatusChanged(NotificationsAuthorizationStatus status)
        {
            if (!Initialized || Autorizing || status != NotificationsAuthorizationStatus.Authorized) 
                return;
            
            if (!TryGetToken(NotificationsTokenType.FirebaseDeviceToken, out _))
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