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
    class Route : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllRoutesForSpaceResponse route;

        public Route(ListAllRoutesForSpaceResponse route, CloudFoundryClient client)
            : base(CloudItemType.Route)
        {
            this.client = client;
            this.route = route;
        }

        public override string Text
        {
            get
            {
                return this.route.Host;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Route;
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

        public string AppsUrl { get { return this.route.AppsUrl; } /*private set;*/ }
        public string DomainGuid { get { return this.route.DomainGuid.ToString(); } /*private set;*/ }
        public string DomainUrl { get { return this.route.DomainUrl; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this.route.EntityMetadata; } /*private set;*/ }
        public string Host { get { return this.route.Host; } /*private set;*/ }
        public string SpaceGuid { get { return this.route.SpaceGuid.ToString(); } /*private set;*/ }
        public string SpaceUrl { get { return this.route.SpaceUrl; } /*private set;*/ }        
    }
}
