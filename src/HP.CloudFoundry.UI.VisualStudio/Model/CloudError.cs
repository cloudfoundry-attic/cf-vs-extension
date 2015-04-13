using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class CloudError : CloudItem
    {
        private Exception exception;
        private List<string> errorMessages;

        public CloudError(Exception exception)
            : base(CloudItemType.Error)
        {
            this.exception = exception;
            this.errorMessages = new List<string>();
            FormatExceptionMessage(this.exception, this.errorMessages);
        }

        private static void FormatExceptionMessage(Exception ex, List<string> message)
        {
            if (ex is AggregateException)
            {
                foreach (Exception iex in (ex as AggregateException).Flatten().InnerExceptions)
                {
                    FormatExceptionMessage(iex, message);
                }
            }
            else
            {
                message.Add(ex.Message);

                if (ex.InnerException != null)
                {
                    FormatExceptionMessage(ex.InnerException, message);
                }
            }
        }

        public string FullError
        {
            get
            {
                return string.Join("\r\n", this.errorMessages);
            }
        }

        public override string Text
        {
            get
            {
                return this.errorMessages.Last();
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Error;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] {};
            });
        }

        public override ObservableCollection<CloudItemAction> Actions
        {
            get { return null; }
        }
    }
}
