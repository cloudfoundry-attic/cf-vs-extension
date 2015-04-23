using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.Model
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

        [Description("Name of the space.")]
        public string Name { get { return _space.Name; } }

        [DisplayName("Space roles")]
        [Description("The space roles for the current user.")]
        public string SpaceRoles
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
