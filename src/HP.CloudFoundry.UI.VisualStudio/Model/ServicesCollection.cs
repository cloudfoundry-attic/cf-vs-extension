using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class ServicesCollection : CloudItem
    {
        private readonly CloudFoundryClient _client;
        private readonly ListAllSpacesForOrganizationResponse _space;

        public ServicesCollection(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            _client = client;
            _space = space;
        }

        public override string Text
        {
            get
            {
                return "Service Instances";
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Services;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Service> result = new List<Service>();

            PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> services = await _client.Spaces.ListAllServiceInstancesForSpace(_space.EntityMetadata.Guid);

            while (services != null && services.Properties.TotalResults != 0)
            {
                foreach (var service in services)
                {
                    result.Add(new Service(service, _client));
                }

                services = await services.GetNextPage();
            }

            return result;
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }
    }
}
