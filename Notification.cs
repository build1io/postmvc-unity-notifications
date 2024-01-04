namespace Build1.PostMVC.Unity.Notifications
{
    public sealed class Notification
    {
        public readonly int    id;
        public readonly string idString;

        public string title;
        public string subTitle;
        public string text;

        public string smallIcon;
        public string largeIcon;

        public int  timeoutSeconds;
        public bool showInForeground = true;

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

            this.timeoutSeconds = timeoutSeconds;
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

            this.timeoutSeconds = timeoutSeconds;
        }
        
        public Notification(int id, string title, string subTitle, string text, string smallIcon, string largeIcon, int timeoutSeconds, bool showInForeground)
        {
            this.id = id;
            this.idString = id.ToString();
            this.title = title;
            this.subTitle = subTitle;
            this.text = text;
            this.smallIcon = smallIcon;
            this.largeIcon = largeIcon;
            this.timeoutSeconds = timeoutSeconds;
            this.showInForeground = showInForeground;
        }
        
        public Notification SetTitle(string value)
        {
            title = value;
            return this;
        }
        
        public Notification SetSubTitle(string value)
        {
            subTitle = value;
            return this;
        }

        public Notification SetText(string value)
        {
            text = value;
            return this;
        }
        
        public Notification SetSmallIcon(string value)
        {
            smallIcon = value;
            return this;
        }
        
        public Notification SetLargeIcon(string value)
        {
            largeIcon = value;
            return this;
        }
        
        public Notification SetTimeout(int value)
        {
            timeoutSeconds = value;
            return this;
        }

        public Notification SetShowInForeground(bool value)
        {
            showInForeground = value;
            return this;
        }

        public Notification Copy()
        {
            return new Notification(id, title, subTitle, text, smallIcon, largeIcon, timeoutSeconds, showInForeground);
        }

        public override string ToString()
        {
            return $"[\"{title}\", \"{text}\"]";
        }
    }
}