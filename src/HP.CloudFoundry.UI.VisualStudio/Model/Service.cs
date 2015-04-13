using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Service : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllServiceInstancesForSpaceResponse service;

        public Service(ListAllServiceInstancesForSpaceResponse service, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            this.client = client;
            this.service = service;
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.service.Name, this.service.Type);
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

        public override ObservableCollection<CloudItemAction> Actions
        {
            get
            {
                return new ObservableCollection<CloudItemAction>()
                {
                    new CloudItemAction("Remove", Resources.StatusStopped, () => {})
                };
            }
        }

        public Dictionary<string, dynamic> Credentials { get { return this.service.Credentials; } /*private set;*/ }
        public string DashboardUrl { get { return this.service.DashboardUrl; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this.service.EntityMetadata; } /*private set;*/ }
        public string GatewayData { get { return this.service.GatewayData.ToString(); } /*private set;*/ }
        public string Name { get { return this.service.Name; } /*private set;*/ }
        public string ServiceBindingsUrl { get { return this.service.ServiceBindingsUrl; } /*private set;*/ }
        public string ServicePlanGuid { get { return this.service.ServicePlanGuid.ToString(); } /*private set;*/ }
        public string ServicePlanUrl { get { return this.service.ServicePlanUrl; } /*private set;*/ }
        public string SpaceGuid { get { return this.service.SpaceGuid.ToString(); } /*private set;*/ }
        public string SpaceUrl { get { return this.service.SpaceUrl; } /*private set;*/ }
        public string Type { get { return this.service.Type; } /*private set;*/ }        
    }
}
