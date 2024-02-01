namespace Build1.PostMVC.Unity.Notifications
{
    public sealed class Notification
    {
        public readonly int id;

        public string Title    { get; set; }
        public string SubTitle { get; set; }
        public string Text     { get; set; }

        public string IconSmall { get; set; }
        public string IconLarge { get; set; }

        public string IOSThreadId    { get; set; }
        public string AndroidGroupId { get; set; }

        public int  TimeoutSeconds   { get; set; }
        public bool ShowInForeground { get; set; } = true;

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
            IconSmall = value;
            return this;
        }

        public Notification SetIconLarge(string value)
        {
            IconLarge = value;
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

        public Notification Copy()
        {
            return new Notification(id, Title, Text)
            {
                SubTitle = SubTitle,
                IconLarge = IconLarge,
                IconSmall = IconSmall,
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