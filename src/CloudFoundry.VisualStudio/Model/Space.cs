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

        public Space(ListAllSpacesForOrganizationResponse space, CloudFoundryClient client)
            : base(CloudItemType.Space)
        {
            _client = client;
            _space = space;
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
