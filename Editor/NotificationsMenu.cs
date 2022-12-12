#if UNITY_EDITOR

using Build1.PostMVC.Unity.Notifications.Impl;
using UnityEditor;

namespace Build1.PostMVC.Unity.Notifications.Editor
{
    public static class NotificationsMenu
    {
        [MenuItem("Tools/Build1/Notifications/View Authorization Status", false, 2113)]
        public static void View()
        {
            var status = NotificationsControllerEditor.GetAuthorizationStatusStatic();
            EditorUtility.DisplayDialog("Notifications", $"Current status: {status}.", "Ok");
        }
        
        [MenuItem("Tools/Build1/Notifications/Reset Editor Authorization", false, 2114)]
        public static void Reset()
        {
            NotificationsControllerEditor.ResetAuthorizationStatusStatic();
            EditorUtility.DisplayDialog("Notifications", "Editor authorization status reset.", "Ok");
        }
    }
}

#endif