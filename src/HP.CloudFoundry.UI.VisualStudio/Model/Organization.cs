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
    class Organization : CloudItem
    {
        private ListAllOrganizationsResponse organization;
        private CloudFoundryClient client;

        public Organization(ListAllOrganizationsResponse org, CloudFoundryClient client)
            : base(CloudItemType.Organization)
        {
            this.client = client;
            this.organization = org;
        }

        public override string Text
        {
            get
            {
                return this.organization.Name;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Organization;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Space> result = new List<Space>();

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await client.Organizations.ListAllSpacesForOrganization(this.organization.EntityMetadata.Guid);

            while (spaces != null && spaces.Properties.TotalResults != 0)
            {
                foreach (var space in spaces)
                {
                    result.Add(new Space(space, this.client));
                }

                spaces = await spaces.GetNextPage();
            }

            return result;
        }
    }
}
