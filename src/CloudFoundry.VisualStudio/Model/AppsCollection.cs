namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;

    internal class AppsCollection : CloudItem
    {
        private readonly CloudFoundryClient client;
        private readonly ListAllSpacesForOrganizationResponse space;

        public AppsCollection(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.AppsCollection)
        {
            this.client = client;
            this.space = space;
        }

        public override string Text
        {
            get
            {
                return "Apps";
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Apps;
            }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<App> result = new List<App>();

            PagedResponseCollection<ListAllAppsForSpaceResponse> apps = await this.client.Spaces.ListAllAppsForSpace(this.space.EntityMetadata.Guid);

            while (apps != null && apps.Properties.TotalResults != 0)
            {
                foreach (var app in apps)
                {
                    var appSummary = await this.client.Apps.GetAppSummary(app.EntityMetadata.Guid);
                    result.Add(new App(appSummary, this.client));
                }

                apps = await apps.GetNextPage();
            }

            return result;
        }
    }
}
