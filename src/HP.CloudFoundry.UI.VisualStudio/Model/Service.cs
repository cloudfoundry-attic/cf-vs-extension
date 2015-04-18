using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Service : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly ListAllServiceInstancesForSpaceResponse _service;

        public Service(ListAllServiceInstancesForSpaceResponse service, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            _client = client;
            _service = service;
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", _service.Name, _service.Type);
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
                return new CloudItem[] {};
            });
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Delete", Resources.Delete, () => { return Task.Delay(0); })
                };
            }
        }

        public Dictionary<string, dynamic> Credentials { get { return _service.Credentials; } }
        public string DashboardUrl { get { return _service.DashboardUrl; } }
        public Metadata EntityMetadata { get { return _service.EntityMetadata; } }
        public string GatewayData { get { return _service.GatewayData.ToString(); } }
        public string Name { get { return _service.Name; } }
        public string ServiceBindingsUrl { get { return _service.ServiceBindingsUrl; } }
        public string ServicePlanGuid { get { return _service.ServicePlanGuid.ToString(); } }
        public string ServicePlanUrl { get { return _service.ServicePlanUrl; } }
        public string SpaceGuid { get { return _service.SpaceGuid.ToString(); } }
        public string SpaceUrl { get { return _service.SpaceUrl; } }
        public string Type { get { return _service.Type; } }        
    }
}
