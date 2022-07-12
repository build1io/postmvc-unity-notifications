namespace Build1.PostMVC.Unity.Modules.Notifications
{
    public sealed class Notification
    {
        public readonly int    id;
        public readonly string idString;

        public readonly string title;
        public readonly string subTitle;
        public readonly string text;

        public readonly string smallIcon;
        public readonly string largeIcon;

        public int  TimeoutSeconds   { get; private set; }
        public bool ShowInForeground { get; private set; }

        public Notification(int id, string title, string text)
        {
            this.id = id;
            this.idString = id.ToString();
            this.title = title;
            this.text = text;
        }

        public Notification(int id, string title, string text, int timeoutSeconds)
        {
            this.id = id;
            this.idString = id.ToString();
            this.title = title;
            this.text = text;

            TimeoutSeconds = timeoutSeconds;
        }

        public Notification(int id, string title, string subTitle, string text, string smallIcon, string largeIcon, int timeoutSeconds)
        {
            this.id = id;
            this.idString = id.ToString();
            this.title = title;
            this.subTitle = subTitle;
            this.text = text;
            this.smallIcon = smallIcon;
            this.largeIcon = largeIcon;

            TimeoutSeconds = timeoutSeconds;
        }

        public Notification SetTimeout(int timeoutSeconds)
        {
            TimeoutSeconds = timeoutSeconds;
            return this;
        }

        public Notification SetShowInForeground(bool value)
        {
            ShowInForeground = value;
            return this;
        }

        public Notification Copy()
        {
            return new Notification(id, title, subTitle, text, smallIcon, largeIcon, TimeoutSeconds);
        }

        public override string ToString()
        {
            return $"[\"{title}\", \"{text}\"]";
        }
    }
}