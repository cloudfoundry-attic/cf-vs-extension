namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;

    internal class Organization : CloudItem
    {
        private readonly ListAllOrganizationsResponse organization;
        private readonly CloudFoundryClient client;

        public Organization(ListAllOrganizationsResponse org, CloudFoundryClient client)
            : base(CloudItemType.Organization)
        {
            this.client = client;
            this.organization = org;
        }

        [Description("Name of the Organization")]
        public string Name
        {
            get
            {
                return this.organization.Name;
            }
        }

        [DisplayName("Creation date")]
        [Description("Date when the organization was created.")]
        public string CreationDate
        {
            get
            {
                return this.organization.EntityMetadata.CreatedAt;
            }
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

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<Space> result = new List<Space>();

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await this.client.Organizations.ListAllSpacesForOrganization(this.organization.EntityMetadata.Guid);

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
