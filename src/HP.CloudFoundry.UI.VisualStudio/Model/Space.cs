using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
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

        public string AppEventsUrl { get { return _space.AppEventsUrl; } }
        public string AppsUrl { get { return _space.AppsUrl; } }
        public string AuditorsUrl { get { return _space.AuditorsUrl; } }
        public string DevelopersUrl { get { return _space.DevelopersUrl; } }
        public string DomainsUrl { get { return _space.DomainsUrl; } }
        public Metadata EntityMetadata { get { return _space.EntityMetadata; } }
        public string EventsUrl { get { return _space.EventsUrl; } }
        public string ManagersUrl { get { return _space.ManagersUrl; } }
        public string Name { get { return _space.Name; } }
        public string OrganizationGuid { get { return _space.OrganizationGuid.ToString(); } }
        public string OrganizationUrl { get { return _space.OrganizationUrl; } }
        public string RoutesUrl { get { return _space.RoutesUrl; } }
        public string SecurityGroupsUrl { get { return _space.SecurityGroupsUrl; } }
        public string ServiceInstancesUrl { get { return _space.ServiceInstancesUrl; } }
        public string SpaceQuotaDefinitionGuid { get { return _space.SpaceQuotaDefinitionGuid.ToString(); } }
    }
}
