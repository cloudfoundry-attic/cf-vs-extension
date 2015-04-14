using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class LoadingPlaceholder : CloudItem
    {
        public LoadingPlaceholder()
            : base(CloudItemType.LoadingPlaceholder)
        {
        }

        public override string Text
        {
            get
            {
                return "Loading ...";
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Refresh;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem>.Run(() =>
                {
                    return new CloudItem[] { };
                });
        }

        public override ObservableCollection<CloudItemAction> Actions
        {
            get { return null; }
        }
    }
}
