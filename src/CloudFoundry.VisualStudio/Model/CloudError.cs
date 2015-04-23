using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.Model
{
    class CloudError : CloudItem
    {
        private Exception _exception;
        private readonly List<string> _errorMessages;

        public CloudError(Exception exception)
            : base(CloudItemType.Error)
        {
            _exception = exception;
            _errorMessages = new List<string>();
            ErrorFormatter.FormatExceptionMessage(_exception, _errorMessages);
        }

        public string FullError
        {
            get
            {
                return string.Join("\r\n", _errorMessages);
            }
        }

        public override string Text
        {
            get
            {
                return _errorMessages.Last();
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

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get { return null; }
        }
    }
}
