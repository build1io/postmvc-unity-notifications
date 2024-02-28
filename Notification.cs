namespace Build1.PostMVC.Unity.Notifications
{
    public sealed class Notification
    {
        public const string AndroidAppIconLarge = "main";

        public readonly int id;

        public string Title    { get; set; }
        public string SubTitle { get; set; }
        public string Text     { get; set; }

        public string AndroidGroupId   { get; set; }
        public string AndroidIconSmall { get; set; }
        public string AndroidIconLarge { get; set; }

        public string IOSThreadId { get; set; }

        public int  TimeoutSeconds   { get; set; }
        public bool ShowInForeground { get; set; } = true;

        public int AppBadgeCount { get; set; }

        public Notification(int id, string title, string text)
        {
            this.id = id;

            Title = title;
            Text = text;
        }

        public Notification SetTitle(string value)
        {
            Title = value;
            return this;
        }

        public Notification SetSubTitle(string value)
        {
            SubTitle = value;
            return this;
        }

        public Notification SetText(string value)
        {
            Text = value;
            return this;
        }

        public Notification SetIconSmall(string value)
        {
            AndroidIconSmall = value;
            return this;
        }

        public Notification SetIconLarge(string value)
        {
            AndroidIconLarge = value;
            return this;
        }

        public Notification SetIOSThreadId(string value)
        {
            IOSThreadId = value;
            return this;
        }

        public Notification SetAndroidGroupId(string value)
        {
            AndroidGroupId = value;
            return this;
        }

        public Notification SetTimeout(int value)
        {
            TimeoutSeconds = value;
            return this;
        }

        public Notification SetShowInForeground(bool value)
        {
            ShowInForeground = value;
            return this;
        }

        public Notification SetAppBadgeCount(int value)
        {
            AppBadgeCount = value;
            return this;
        }

        public Notification Copy()
        {
            return new Notification(id, Title, Text)
            {
                SubTitle = SubTitle,
                AndroidIconLarge = AndroidIconLarge,
                AndroidIconSmall = AndroidIconSmall,
                IOSThreadId = IOSThreadId,
                AndroidGroupId = AndroidGroupId,
                TimeoutSeconds = TimeoutSeconds,
                ShowInForeground = ShowInForeground
            };
        }

        public override string ToString()
        {
            return $"[\"{Title}\", \"{Text}\"]";
        }
    }
}