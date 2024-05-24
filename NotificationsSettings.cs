namespace Build1.PostMVC.Unity.Notifications
{
    public record NotificationsSettings(NotificationsSetting Settings)
    {
        public string DefaultSoundName { get; private set; }

        public NotificationsSettings SetDefaultSoundName(string soundName)
        {
            DefaultSoundName = soundName;
            return this;
        }
    }
}