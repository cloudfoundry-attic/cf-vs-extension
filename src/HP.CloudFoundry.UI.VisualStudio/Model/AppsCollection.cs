using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class AppsCollection : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllSpacesForOrganizationResponse space;

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

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<App> result = new List<App>();

            PagedResponseCollection<ListAllAppsForSpaceResponse> apps = await client.Spaces.ListAllAppsForSpace(this.space.EntityMetadata.Guid);

            while (apps != null && apps.Properties.TotalResults != 0)
            {
                foreach (var app in apps)
                {
                    result.Add(new App(app, this.client));
                }

                apps = await apps.GetNextPage();
            }

            return result;
        }
    }
}
