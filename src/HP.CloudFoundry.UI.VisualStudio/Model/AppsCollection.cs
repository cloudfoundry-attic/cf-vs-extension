using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class AppsCollection : CloudItem
    {
        private readonly CloudFoundryClient _client;
        private readonly ListAllSpacesForOrganizationResponse _space;

        public AppsCollection(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.AppsCollection)
        {
            _client = client;
            _space = space;
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

            PagedResponseCollection<ListAllAppsForSpaceResponse> apps = await _client.Spaces.ListAllAppsForSpace(this._space.EntityMetadata.Guid);

            while (apps != null && apps.Properties.TotalResults != 0)
            {
                foreach (var app in apps)
                {
                    result.Add(new App(app, this._client));
                }

                apps = await apps.GetNextPage();
            }

            return result;
        }

        public override ObservableCollection<CloudItemAction> Actions
        {
            get
            {
                return null;
            }
        }
    }
}
