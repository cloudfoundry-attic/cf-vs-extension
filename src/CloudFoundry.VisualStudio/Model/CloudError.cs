namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    internal class CloudError : CloudItem
    {
        private readonly List<string> errorMessages;
        private Exception exception;

        public CloudError(Exception exception)
            : base(CloudItemType.Error)
        {
            this.exception = exception;
            this.errorMessages = new List<string>();
            ErrorFormatter.FormatExceptionMessage(this.exception, this.errorMessages);
        }

        public string FullError
        {
            get { return string.Join("\r\n", this.errorMessages); }
        }

        public override string Text
        {
            get { return this.errorMessages.Last(); }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get { return Resources.Error; }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get { return null; }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }
    }
}
