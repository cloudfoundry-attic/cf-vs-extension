using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
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
    }
}
