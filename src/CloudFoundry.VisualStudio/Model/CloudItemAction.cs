namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    internal class CloudItemAction
    {
        private readonly string text;
        private readonly Bitmap icon;
        private readonly Func<Task> onClick;
        private readonly CloudItem cloudItem;
        private CloudItemActionContinuation continuation;

        public CloudItemAction(CloudItem cloudItem, string text, Bitmap icon, Func<Task> onClick)
            : this(cloudItem, text, icon, onClick, CloudItemActionContinuation.None)
        {
        }

        public CloudItemAction(CloudItem cloudItem, string text, Bitmap icon, Func<Task> onClick, CloudItemActionContinuation continuation)
        {
            this.cloudItem = cloudItem;
            this.continuation = continuation;
            this.text = text;
            this.icon = icon;
            this.onClick = onClick;
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public CloudItemActionContinuation Continuation
        {
            get
            {
                return this.continuation;
            }
        }

        public Func<Task> Click
        {
            get
            {
                return this.onClick;
            }
        }

        public CloudItem CloudItem
        {
            get
            {
                return this.cloudItem;
            }
        }

        public System.Windows.Controls.Image Icon
        {
            get
            {
                var bitmapImage = Converters.ImageConverter.ConvertBitmapToBitmapImage(this.icon);

                if (bitmapImage != null)
                {
                    return new System.Windows.Controls.Image()
                    {
                        Source = bitmapImage
                    };
                }

                return null;
            }
        }
    }
}
