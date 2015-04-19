using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using HP.CloudFoundry.UI.VisualStudio.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Service : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly ListAllServiceInstancesForSpaceResponse _service;
        private readonly ICollection<GetAppSummaryResponse> _appsSummary;
        private readonly RetrieveServicePlanResponse _servicePlan;
        private readonly RetrieveServiceResponse _systemService;
        private readonly PagedResponseCollection<ListAllServiceBindingsForServiceInstanceResponse> _serviceBindings;

        public Service(ListAllServiceInstancesForSpaceResponse service, ICollection<GetAppSummaryResponse> appsSummary,
            RetrieveServicePlanResponse servicePlan, RetrieveServiceResponse systemService,
            PagedResponseCollection<ListAllServiceBindingsForServiceInstanceResponse> serviceBindings, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            _client = client;
            _service = service;
            _appsSummary = appsSummary;
            _servicePlan = servicePlan;
            _systemService = systemService;
            _serviceBindings = serviceBindings;

        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", _service.Name, _systemService.Label);
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Service;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Delete", Resources.Delete, Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }


        private async Task Delete()
        {
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete service '{0}'?",
                this._service.Name
                ));

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (var serviceBinding in _serviceBindings)
                {
                    await this._client.ServiceBindings.DeleteServiceBinding(serviceBinding.EntityMetadata.Guid);
                }

                await this._client.ServiceInstances.DeleteServiceInstance(this._service.EntityMetadata.Guid);

            }
        }

        [Description("The name of the service.")]
        public string Name { get { return _service.Name; } }

        [DisplayName("Bound apps")]
        [Description("Apps that have this service bound.")]
        public string BoundApps
        {
            get
            {
                string boundApps = string.Empty;

                foreach (var appSummary in _appsSummary)
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
                return this._service.EntityMetadata.CreatedAt;
            }
        }

        [DisplayName("Service plan")]
        [Description("The name of the plan.")]
        public string ServicePlan
        {
            get
            {
                return this._servicePlan.Name;
            }
        }

        [DisplayName("Service type")]
        [Description("Type of the service instance.")]
        public string ServiceType
        {
            get
            {
                return this._systemService.Label;
            }
        }
    }
}
