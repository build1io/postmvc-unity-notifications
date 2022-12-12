#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class NotificationsControllerEditor : NotificationsControllerBase
    {
        private const string AuthorizationSetPlayerPrefsKey = "PostMVC_NotificationsControllerEditor_AuthorizationSet";
        
        protected override bool RemoteNotificationsAuthorizationRequired => false;

        /*
         * Authorization.
         */

        protected override NotificationsAuthorizationStatus GetAuthorizationStatus()
        {
            return GetAuthorizationStatusStatic();
        }

        protected override void RequestAuthorization(Notification notification)
        {
            Log.Debug("Editor simulation. Showing authorization editor dialog...");

            var status = EditorUtility.DisplayDialog("Notifications", "Would you like to allow notifications?", "Allow", "Don't allow")
                             ? NotificationsAuthorizationStatus.Authorized
                             : NotificationsAuthorizationStatus.Denied;

            PlayerPrefs.SetInt(AuthorizationSetPlayerPrefsKey, (int)status);
            
            CompleteAuthorization(status, notification);
        }

        /*
         * Scheduling.
         */
        
        protected override void OnScheduleNotification(Notification notification)
        {
            Log.Debug(n => $"Editor simulation. Scheduling notification: {n}", notification);
        }
        
        /*
         * Cancelling.
         */

        protected override void OnCancelScheduledNotification(Notification notification)
        {
            Log.Debug(n => $"Editor simulation. Cancelling scheduled notification: {n}", notification);
        }

        protected override void OnCancelAllScheduledNotifications()
        {
            Log.Debug("Editor simulation. Cancelling scheduled notifications.");
        }
        
        /*
         * Cleaning.
         */

        protected override void OnCleanDisplayedNotifications()
        {
            Log.Debug("Editor simulation. Cleaning displayed notifications.");
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

        public static void ResetAuthorizationStatusStatic()
        {
            PlayerPrefs.DeleteKey(AuthorizationSetPlayerPrefsKey);
        }
    }
}

#endif