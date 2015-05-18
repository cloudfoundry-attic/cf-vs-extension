using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.Model
{
    class Organization : CloudItem
    {
        private readonly ListAllOrganizationsResponse _organization;
        private readonly CloudFoundryClient _client;

        public Organization(ListAllOrganizationsResponse org, CloudFoundryClient client)
            : base(CloudItemType.Organization)
        {
            _client = client;
            _organization = org;
        }

        public override string Text
        {
            get
            {
                return _organization.Name;
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

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await _client.Organizations.ListAllSpacesForOrganization(_organization.EntityMetadata.Guid);

            while (spaces != null && spaces.Properties.TotalResults != 0)
            {
                foreach (var space in spaces)
                {
                    result.Add(new Space(space, this._client));
                }

                spaces = await spaces.GetNextPage();
            }

            return result;
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        [Description("Name of the Organization")]
        public string Name { get { return _organization.Name; } }

        [DisplayName("Creation date")]
        [Description("Date when the organization was created.")]
        public string CreationDate { get { return this._organization.EntityMetadata.CreatedAt; } }

    }
}
