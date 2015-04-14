using System;
using System.Drawing;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    internal class CloudItemAction
    {
        private readonly string _text;
        private readonly Bitmap _icon;
        private readonly Action _onClick;
        
        public CloudItemAction(string text, Bitmap icon, Action onClick)
        {
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

        public Action Click
        {
            get
            {
                return _onClick;
            }
        }

        public System.Windows.Controls.Image Icon
        {
            get
            {
                var bitmapImage = ImageConverter.ConvertBitmapToBitmapImage(_icon);

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
