using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
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
                return new CloudItemAction[]
                {
                    new CloudItemAction("Remove", Resources.StatusStopped, () => {})
                };
            }
        }

        public string AppEventsUrl { get { return _organization.AppEventsUrl; } }
        public string AuditorsUrl { get { return _organization.AuditorsUrl; } }
        public bool BillingEnabled { get { return _organization.BillingEnabled; } }
        public string BillingManagersUrl { get { return _organization.BillingManagersUrl; } }
        public string DomainsUrl { get { return _organization.DomainsUrl; } }
        public Metadata EntityMetadata { get { return _organization.EntityMetadata; } }
        public string ManagersUrl { get { return _organization.ManagersUrl; } }
        public string Name { get { return _organization.Name; } }
        public string PrivateDomainsUrl { get { return _organization.PrivateDomainsUrl; } }
        public string QuotaDefinitionGuid { get { return _organization.QuotaDefinitionGuid.ToString(); } }
        public string QuotaDefinitionUrl { get { return _organization.QuotaDefinitionUrl; } }
        public string SpaceQuotaDefinitionsUrl { get { return _organization.SpaceQuotaDefinitionsUrl; } }
        public string SpacesUrl { get { return _organization.SpacesUrl; } }
        public string Status { get { return _organization.Status; } }
        public string UsersUrl { get { return _organization.UsersUrl; } }
    }
}
