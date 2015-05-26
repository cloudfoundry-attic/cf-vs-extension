namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    internal class LoadingPlaceholder : CloudItem
    {
        public LoadingPlaceholder()
            : base(CloudItemType.LoadingPlaceholder)
        {
        }

        public override string Text
        {
            get { return "Loading ..."; }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get { return Resources.Synchronizing; }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get { return null; }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }
    }
}
