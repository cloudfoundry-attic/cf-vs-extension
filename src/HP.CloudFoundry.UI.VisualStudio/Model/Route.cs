using System.Globalization;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Route : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly ListAllRoutesForSpaceResponse _route;

        public Route(ListAllRoutesForSpaceResponse route, CloudFoundryClient client)
            : base(CloudItemType.Route)
        {
            _client = client;
            _route = route;
        }

        public override string Text
        {
            get
            {
                return _route.Host;
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

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction("Remove", Resources.StatusStopped, () => {})
                };
            }
        }

        public string AppsUrl { get { return _route.AppsUrl; } }
        public string DomainGuid { get { return _route.DomainGuid.ToString(); } }
        public string DomainUrl { get { return _route.DomainUrl; } }
        public Metadata EntityMetadata { get { return _route.EntityMetadata; } }
        public string Host { get { return _route.Host; } }
        public string SpaceGuid { get { return _route.SpaceGuid.ToString(); } }
        public string SpaceUrl { get { return _route.SpaceUrl; } }        
    }
}
