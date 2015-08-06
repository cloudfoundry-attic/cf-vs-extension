namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;

    internal class ServicesCollection : CloudItem
    {
        private readonly CloudFoundryClient client;
        private readonly ListAllSpacesForOrganizationResponse space;

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

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Service> result = new List<Service>();

            PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> services = await this.client.Spaces.ListAllServiceInstancesForSpace(this.space.EntityMetadata.Guid);

            while (services != null && services.Properties.TotalResults != 0)
            {
                foreach (var service in services)
                {
                    List<GetAppSummaryResponse> appsSummary = new List<GetAppSummaryResponse>();
                    var serviceBindings = await this.client.ServiceInstances.ListAllServiceBindingsForServiceInstance(service.EntityMetadata.Guid);
                    if (serviceBindings.Properties.TotalResults != 0)
                    {
                        foreach (var serviceBinding in serviceBindings)
                        {
                            appsSummary.Add(await this.client.Apps.GetAppSummary(serviceBinding.AppGuid));
                        }
                    }

                    var servicePlan = await this.client.ServicePlans.RetrieveServicePlan(service.ServicePlanGuid);
                    var systemService = await this.client.Services.RetrieveService(servicePlan.ServiceGuid);

                    result.Add(new Service(service, appsSummary, servicePlan, systemService, this.client));
                }

                services = await services.GetNextPage();
            }

            return result;
        }
    }
}
