using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class RoutesCollection : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllSpacesForOrganizationResponse space;

        public RoutesCollection(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.RoutesCollection)
        {
            this.client = client;
            this.space = space;
        }

        public override string Text
        {
            get
            {
                return "Routes";
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Routes;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Route> result = new List<Route>();

            PagedResponseCollection<ListAllRoutesForSpaceResponse> routes = await client.Spaces.ListAllRoutesForSpace(this.space.EntityMetadata.Guid);

            while (routes != null && routes.Properties.TotalResults != 0)
            {
                foreach (var route in routes)
                {
                    result.Add(new Route(route, this.client));
                }

                routes = await routes.GetNextPage();
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
