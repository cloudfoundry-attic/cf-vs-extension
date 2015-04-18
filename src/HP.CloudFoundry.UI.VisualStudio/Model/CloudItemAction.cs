using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    internal class CloudItemAction
    {
        private readonly string _text;
        private readonly Bitmap _icon;
        private readonly Func<Task> _onClick;
        private readonly CloudItem _cloudItem;
        
        public CloudItemAction(CloudItem cloudItem, string text, Bitmap icon, Func<Task> onClick)
        {
            this._cloudItem = cloudItem;
            _text = text;
            _icon = icon;
            _onClick = onClick;
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public Func<Task> Click
        {
            get
            {
                return _onClick;
            }
        }

        public CloudItem CloudItem
        {
            get
            {
                return _cloudItem;
            }
        }

        public System.Windows.Controls.Image Icon
        {
            get
            {
                var bitmapImage = Converters.ImageConverter.ConvertBitmapToBitmapImage(_icon);

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
