namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.VisualStudio.Forms;

    internal class Service : CloudItem
    {
        private readonly ListAllServiceInstancesForSpaceResponse service;
        private readonly ICollection<GetAppSummaryResponse> appsSummary;
        private readonly RetrieveServicePlanResponse servicePlan;
        private readonly RetrieveServiceResponse systemService;
        private CloudFoundryClient client;

        public Service(
            ListAllServiceInstancesForSpaceResponse service,
            ICollection<GetAppSummaryResponse> appsSummary,
            RetrieveServicePlanResponse servicePlan,
            RetrieveServiceResponse systemService,
            CloudFoundryClient client)
            : base(CloudItemType.Service)
        {
            this.client = client;
            this.service = service;
            this.appsSummary = appsSummary;
            this.servicePlan = servicePlan;
            this.systemService = systemService;
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.service.Name, this.systemService.Label);
            }
        }

        [Description("The name of the service.")]
        public string Name
        {
            get
            {
                return this.service.Name;
            }
        }

        [DisplayName("Bound apps")]
        [Description("Apps that have this service bound.")]
        public string BoundApps
        {
            get
            {
                string boundApps = string.Empty;

                foreach (var appSummary in this.appsSummary)
                {
                    boundApps = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", appSummary.Name, boundApps);
                }

                return boundApps.Trim().TrimEnd(',');
            }
        }

        [DisplayName("Creation date")]
        [Description("Date when the service was created.")]
        public string CreationDate
        {
            get
            {
                return this.service.EntityMetadata.CreatedAt;
            }
        }

        [DisplayName("Service plan")]
        [Description("The name of the plan.")]
        public string ServicePlan
        {
            get
            {
                return this.servicePlan.Name;
            }
        }

        [DisplayName("Service type")]
        [Description("Type of the service instance.")]
        public string ServiceType
        {
            get
            {
                return this.systemService.Label;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Service;
            }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Delete", Resources.Delete, this.Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }

        private async Task Delete()
        {
            try
            {
                this.EnableNodes(this.service.EntityMetadata.Guid, false);
                var answer = MessageBoxHelper.WarningQuestion(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "Are you sure you want to delete service '{0}'?",
                    this.service.Name));

                if (answer == System.Windows.Forms.DialogResult.Yes)
                {
                    var serviceBindings = await this.client.ServiceInstances.ListAllServiceBindingsForServiceInstance(this.service.EntityMetadata.Guid);
                    if (serviceBindings.Properties.TotalResults != 0)
                    {
                        foreach (var serviceBinding in serviceBindings)
                        {
                            await this.client.ServiceBindings.DeleteServiceBinding(serviceBinding.EntityMetadata.Guid);
                        }
                    }

                    await this.client.ServiceInstances.DeleteServiceInstance(this.service.EntityMetadata.Guid);
                }
            }
            finally
            {
                this.EnableNodes(this.service.EntityMetadata.Guid, false);
            }
        }
    }
}
