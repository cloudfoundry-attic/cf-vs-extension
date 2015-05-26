namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;

    internal class RoutesCollection : CloudItem
    {
        private readonly CloudFoundryClient client;
        private readonly ListAllSpacesForOrganizationResponse space;

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

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Route> result = new List<Route>();

            PagedResponseCollection<ListAllRoutesForSpaceResponse> routes = await this.client.Spaces.ListAllRoutesForSpace(this.space.EntityMetadata.Guid);

            while (routes != null && routes.Properties.TotalResults != 0)
            {
                foreach (var route in routes)
                {
                    var routeApps = await this.client.Routes.ListAllAppsForRoute(route.EntityMetadata.Guid);

                    var domain = await this.client.DomainsDeprecated.RetrieveDomainDeprecated(route.DomainGuid);
                    result.Add(new Route(route, domain, routeApps, this.client));
                }

                routes = await routes.GetNextPage();
            }

            return result;
        }
    }
}
