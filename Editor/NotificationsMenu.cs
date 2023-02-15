#if UNITY_EDITOR

using Build1.PostMVC.Unity.Notifications.Impl;
using UnityEditor;

namespace Build1.PostMVC.Unity.Notifications.Editor
{
    public static class NotificationsMenu
    {
        private const string EnabledMenuItem  = "Tools/Build1/PostMVC/Notifications/Enable";
        private const string DisabledMenuItem = "Tools/Build1/PostMVC/Notifications/Disable";
        private const string ResetMenuItem    = "Tools/Build1/PostMVC/Notifications/Reset";

        [MenuItem(EnabledMenuItem, false, 10)]
        public static void Enable()
        {
            NotificationsControllerEditor.SetAuthorizationStatusStatic(NotificationsAuthorizationStatus.Authorized);
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(EnabledMenuItem, true, 10)]
        public static bool EnableValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.Authorized;
        }

        [MenuItem(DisabledMenuItem, false, 11)]
        public static void Disable()
        {
            NotificationsControllerEditor.SetAuthorizationStatusStatic(NotificationsAuthorizationStatus.Denied);
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(DisabledMenuItem, true, 11)]
        public static bool DisableValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.Denied;
        }

        [MenuItem(ResetMenuItem, false, 30)]
        public static void Reset()
        {
            NotificationsControllerEditor.ResetAuthorizationStatusStatic();
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(ResetMenuItem, true, 30)]
        public static bool ResetValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.NotDetermined;
        }
    }
}

#endif