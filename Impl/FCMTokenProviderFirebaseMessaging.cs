using System;
using Build1.PostMVC.Unity.App.Modules.Logging;
using Firebase.Extensions;
using Firebase.Messaging;

namespace Build1.PostMVC.Unity.Notifications.Impl
{
    internal sealed class FCMTokenProviderFirebaseMessaging : IFCMTokenProvider
    {
        [Log(LogLevel.Warning)] public ILog Log { get; set; }

        public void GetToken(Action<string> onComplete)
        {
            Log.Debug("Loading FCM token...");
            
            FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
            {
                var token = task.Result;
                
                Log.Debug(t => $"FCM token loaded. Token: {t}", token);
                
                onComplete.Invoke(token);
            });
        }
    }
}