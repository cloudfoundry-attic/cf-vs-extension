using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Organization : CloudItem
    {
        private readonly ListAllOrganizationsResponse _organization;
        private readonly CloudFoundryClient _client;
        private readonly GetUserSummaryResponse _userSumary;

        public Organization(ListAllOrganizationsResponse org, GetUserSummaryResponse userSummary, CloudFoundryClient client)
            : base(CloudItemType.Organization)
        {
            _client = client;
            _organization = org;
            _userSumary = userSummary;
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
                    result.Add(new Space(space, this._userSumary, this._client));
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

        [Browsable(false)]
        public string AppEventsUrl { get { return _organization.AppEventsUrl; } }

        [Browsable(false)]
        public string AuditorsUrl { get { return _organization.AuditorsUrl; } }

        [Browsable(false)]
        public bool BillingEnabled { get { return _organization.BillingEnabled; } }

        [Browsable(false)]
        public string BillingManagersUrl { get { return _organization.BillingManagersUrl; } }

        [Browsable(false)]
        public string DomainsUrl { get { return _organization.DomainsUrl; } }

        [Browsable(false)]
        public Metadata EntityMetadata { get { return _organization.EntityMetadata; } }

        [Browsable(false)]
        public string ManagersUrl { get { return _organization.ManagersUrl; } }

        [Description("Name of the Organization")]
        public string Name { get { return _organization.Name; } }

        [Browsable(false)]
        public string PrivateDomainsUrl { get { return _organization.PrivateDomainsUrl; } }

        [Browsable(false)]
        public string QuotaDefinitionGuid { get { return _organization.QuotaDefinitionGuid.ToString(); } }

        [Browsable(false)]
        public string QuotaDefinitionUrl { get { return _organization.QuotaDefinitionUrl; } }

        [Browsable(false)]
        public string SpaceQuotaDefinitionsUrl { get { return _organization.SpaceQuotaDefinitionsUrl; } }

        [Browsable(false)]
        public string SpacesUrl { get { return _organization.SpacesUrl; } }

        [Browsable(false)]
        public string Status { get { return _organization.Status; } }

        [Browsable(false)]
        public string UsersUrl { get { return _organization.UsersUrl; } }

        [DisplayName("Creation date")]
        [Description("Date when the organization was created.")]
        public string CreationDate { get { return this._organization.EntityMetadata.CreatedAt; } }

        [DisplayName("Organization roles")]
        [Description("The roles of the user in the organization")]
        public string OrgRoles
        {
            get
            {
                string orgRoles = string.Empty;


                if (HasRole(_userSumary.Organizations, this._organization.EntityMetadata.Guid.ToString()))
                {
                    orgRoles = string.Format(CultureInfo.InvariantCulture, "Member, {0}", orgRoles);
                }

                if (HasRole(_userSumary.AuditedOrganizations, this._organization.EntityMetadata.Guid.ToString()))
                {
                    orgRoles = string.Format(CultureInfo.InvariantCulture, "Auditor, {0}", orgRoles);
                }

                if (HasRole(_userSumary.BillingManagedOrganizations, this._organization.EntityMetadata.Guid.ToString()))
                {
                    orgRoles = string.Format(CultureInfo.InvariantCulture, "Billing Manager, {0}", orgRoles);
                }

                if (HasRole(_userSumary.ManagedOrganizations, this._organization.EntityMetadata.Guid.ToString()))
                {
                    orgRoles = string.Format(CultureInfo.InvariantCulture, "Manager, {0}", orgRoles);
                }

                return orgRoles.Trim().TrimEnd(',');
            }
        }

        internal static bool HasRole(Dictionary<string, dynamic>[] orgRoles, string id)
        {
            bool exist = false;

            foreach (var org in orgRoles)
            {
                if (org["metadata"]["guid"].ToString() == id)
                {
                    return true;
                }
            }

            return exist;
        }
    }
}
