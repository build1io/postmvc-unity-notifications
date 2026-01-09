using System;

namespace Build1.PostMVC.Unity.Notifications
{
    public interface IFCMTokenProvider
    {
        void GetToken(Action<string> onComplete);
    }
}