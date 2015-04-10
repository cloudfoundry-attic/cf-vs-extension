using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Space : CloudItem
    {
        private ListAllSpacesForOrganizationResponse space;
        private CloudFoundryClient client;

        public Space(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.Space)
        {
            this.client = client;
            this.space = space;
        }

        public override string Text
        {
            get
            {
                return this.space.Name;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Space;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] {
                    new AppsCollection(this.space, this.client),
                    new ServicesCollection(this.space, this.client),
                    new RoutesCollection(this.space, this.client),
                };
            });
        }
    }
}
