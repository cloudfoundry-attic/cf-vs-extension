using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    internal class CloudItemAction
    {
        private string text;
        private Bitmap icon;
        private Action onClick;
        
        public CloudItemAction(string text, Bitmap icon, Action onClick)
        {
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

        public Action Click
        {
            get
            {
                return this.onClick;
            }
        }

        public System.Windows.Controls.Image Icon
        {
            get
            {
                var bitmapImage = ImageConverter.ConvertBitmapToBitmapImage(this.icon);

                if (bitmapImage != null)
                {
                    return new System.Windows.Controls.Image()
                    {
                        Source = bitmapImage
                    };
                }
                else
                { 
                    return null;
                }
            }
        }
    }
}
