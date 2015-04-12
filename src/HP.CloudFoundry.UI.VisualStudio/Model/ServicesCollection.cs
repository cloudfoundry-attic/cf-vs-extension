using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class ServicesCollection : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllSpacesForOrganizationResponse space;

        public ServicesCollection(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            this.client = client;
            this.space = space;
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

            PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> services = await client.Spaces.ListAllServiceInstancesForSpace(this.space.EntityMetadata.Guid);

            while (services != null && services.Properties.TotalResults != 0)
            {
                foreach (var service in services)
                {
                    result.Add(new Service(service, this.client));
                }

                services = await services.GetNextPage();
            }

            return result;
        }
    }
}
