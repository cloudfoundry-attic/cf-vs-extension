namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;

    internal class Space : CloudItem
    {
        private readonly ListAllSpacesForOrganizationResponse space;
        private readonly CloudFoundryClient client;

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

        [Description("Name of the space.")]
        public string Name
        {
            get
            {
                return this.space.Name;
            }
        }

        [DisplayName("Creation date")]
        [Description("Date when the space was created.")]
        public string CreatedAt
        {
            get
            {
                return this.space.EntityMetadata.CreatedAt;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Space;
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
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] 
                {
                    new AppsCollection(space, client),
                    new ServicesCollection(space, client),
                    new RoutesCollection(space, client),
                };
            });
        }
    }
}
