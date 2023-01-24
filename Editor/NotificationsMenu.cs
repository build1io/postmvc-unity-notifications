#if UNITY_EDITOR

using Build1.PostMVC.Unity.Notifications.Impl;
using UnityEditor;

namespace Build1.PostMVC.Unity.Notifications.Editor
{
    public static class NotificationsMenu
    {
        private const string EnabledMenuItem  = "Tools/Build1/Notifications/Enable";
        private const string DisabledMenuItem = "Tools/Build1/Notifications/Disable";
        private const string ResetMenuItem    = "Tools/Build1/Notifications/Reset";

        [MenuItem(EnabledMenuItem, false, 2113)]
        public static void Enable()
        {
            NotificationsControllerEditor.SetAuthorizationStatusStatic(NotificationsAuthorizationStatus.Authorized);
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(EnabledMenuItem, true, 2113)]
        public static bool EnableValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.Authorized;
        }

        [MenuItem(DisabledMenuItem, false, 2114)]
        public static void Disable()
        {
            NotificationsControllerEditor.SetAuthorizationStatusStatic(NotificationsAuthorizationStatus.Denied);
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(DisabledMenuItem, true, 2114)]
        public static bool DisableValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.Denied;
        }

        [MenuItem(ResetMenuItem, false, 2134)]
        public static void Reset()
        {
            NotificationsControllerEditor.ResetAuthorizationStatusStatic();
            EditorUtility.DisplayDialog("Notifications", $"Editor authorization status set: {NotificationsControllerEditor.GetAuthorizationStatusStatic()}", "Ok");
        }

        [MenuItem(ResetMenuItem, true, 2134)]
        public static bool ResetValidation()
        {
            return NotificationsControllerEditor.GetAuthorizationStatusStatic() != NotificationsAuthorizationStatus.NotDetermined;
        }
    }
}

#endif