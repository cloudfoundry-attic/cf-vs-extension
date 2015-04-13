using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public string AppEventsUrl { get { return this.organization.AppEventsUrl; } /*private set;*/ }
        public string AuditorsUrl { get { return this.organization.AuditorsUrl; } /*private set;*/ }
        public bool BillingEnabled { get { return this.organization.BillingEnabled; } /*private set;*/ }
        public string BillingManagersUrl { get { return this.organization.BillingManagersUrl; } /*private set;*/ }
        public string DomainsUrl { get { return this.organization.DomainsUrl; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this.organization.EntityMetadata; } /*private set;*/ }
        public string ManagersUrl { get { return this.organization.ManagersUrl; } /*private set;*/ }
        public string Name { get { return this.organization.Name; } /*private set;*/ }
        public string PrivateDomainsUrl { get { return this.organization.PrivateDomainsUrl; } /*private set;*/ }
        public string QuotaDefinitionGuid { get { return this.organization.QuotaDefinitionGuid.ToString(); } /*private set;*/ }
        public string QuotaDefinitionUrl { get { return this.organization.QuotaDefinitionUrl; } /*private set;*/ }
        public string SpaceQuotaDefinitionsUrl { get { return this.organization.SpaceQuotaDefinitionsUrl; } /*private set;*/ }
        public string SpacesUrl { get { return this.organization.SpacesUrl; } /*private set;*/ }
        public string Status { get { return this.organization.Status; } /*private set;*/ }
        public string UsersUrl { get { return this.organization.UsersUrl; } /*private set;*/ }
    }
}
