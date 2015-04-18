using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Space : CloudItem
    {
        private readonly ListAllSpacesForOrganizationResponse _space;
        private readonly CloudFoundryClient _client;
        private readonly GetUserSummaryResponse _userSummary;

        public Space(ListAllSpacesForOrganizationResponse space, GetUserSummaryResponse userSummary, CloudFoundryClient client)
            : base(CloudItemType.Space)
        {
            _client = client;
            _space = space;
            _userSummary = userSummary;
        }

        public override string Text
        {
            get
            {
                return _space.Name;
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
                    new AppsCollection(_space, _client),
                    new ServicesCollection(_space, _client),
                    new RoutesCollection(_space, _client),
                };
            });
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return null;
            }
        }

        [Browsable(false)]
        public string AppEventsUrl { get { return _space.AppEventsUrl; } }

        [Browsable(false)]
        public string AppsUrl { get { return _space.AppsUrl; } }

        [Browsable(false)]
        public string AuditorsUrl { get { return _space.AuditorsUrl; } }

        [Browsable(false)]
        public string DevelopersUrl { get { return _space.DevelopersUrl; } }

        [Browsable(false)]
        public string DomainsUrl { get { return _space.DomainsUrl; } }

        [Browsable(false)]
        public Metadata EntityMetadata { get { return _space.EntityMetadata; } }

        [Browsable(false)]
        public string EventsUrl { get { return _space.EventsUrl; } }

        [Browsable(false)]
        public string ManagersUrl { get { return _space.ManagersUrl; } }

        [Description("Name of the space.")]
        public string Name { get { return _space.Name; } }

        [Browsable(false)]
        public string OrganizationGuid { get { return _space.OrganizationGuid.ToString(); } }

        [Browsable(false)]
        public string OrganizationUrl { get { return _space.OrganizationUrl; } }

        [Browsable(false)]
        public string RoutesUrl { get { return _space.RoutesUrl; } }

        [Browsable(false)]
        public string SecurityGroupsUrl { get { return _space.SecurityGroupsUrl; } }

        [Browsable(false)]
        public string ServiceInstancesUrl { get { return _space.ServiceInstancesUrl; } }

        [Browsable(false)]
        public string SpaceQuotaDefinitionGuid { get { return _space.SpaceQuotaDefinitionGuid.ToString(); } }

        [DisplayName("Space roles")]
        [Description("The space roles for the current user.")]
        public string SpareRoles
        {
            get
            {
                string spaceRoles = string.Empty;

                if (Organization.HasRole(_userSummary.Spaces, this._space.EntityMetadata.Guid.ToString()))
                {
                    spaceRoles = string.Format(CultureInfo.InvariantCulture, "Developer, {0}", spaceRoles);
                }

                if (Organization.HasRole(_userSummary.AuditedSpaces, this._space.EntityMetadata.Guid.ToString()))
                {
                    spaceRoles = string.Format(CultureInfo.InvariantCulture, "Auditor, {0}", spaceRoles);
                }

                if (Organization.HasRole(_userSummary.ManagedSpaces, this._space.EntityMetadata.Guid.ToString()))
                {
                    spaceRoles = string.Format(CultureInfo.InvariantCulture, "Manager, {0}", spaceRoles);
                }

                return spaceRoles.Trim().TrimEnd(',');
            }
        }

        [DisplayName("Creation date")]
        [Description("Date when the space was created.")]
        public string CreatedAt
        {
            get
            {
                return this._space.EntityMetadata.CreatedAt;
            }
        }

    }

}
