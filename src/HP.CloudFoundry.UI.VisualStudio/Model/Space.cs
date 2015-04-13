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
    class Space : CloudItem
    {
        private ListAllSpacesForOrganizationResponse space;
        private CloudFoundryClient client;

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
                    new AppsCollection(this.space, this.client),
                    new ServicesCollection(this.space, this.client),
                    new RoutesCollection(this.space, this.client),
                };
            });
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

        public string AppEventsUrl { get { return this.space.AppEventsUrl; } /*private set;*/ }
        public string AppsUrl { get { return this.space.AppsUrl; } /*private set;*/ }
        public string AuditorsUrl { get { return this.space.AuditorsUrl; } /*private set;*/ }
        public string DevelopersUrl { get { return this.space.DevelopersUrl; } /*private set;*/ }
        public string DomainsUrl { get { return this.space.DomainsUrl; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this.space.EntityMetadata; } /*private set;*/ }
        public string EventsUrl { get { return this.space.EventsUrl; } /*private set;*/ }
        public string ManagersUrl { get { return this.space.ManagersUrl; } /*private set;*/ }
        public string Name { get { return this.space.Name; } /*private set;*/ }
        public string OrganizationGuid { get { return this.space.OrganizationGuid.ToString(); } /*private set;*/ }
        public string OrganizationUrl { get { return this.space.OrganizationUrl; } /*private set;*/ }
        public string RoutesUrl { get { return this.space.RoutesUrl; } /*private set;*/ }
        public string SecurityGroupsUrl { get { return this.space.SecurityGroupsUrl; } /*private set;*/ }
        public string ServiceInstancesUrl { get { return this.space.ServiceInstancesUrl; } /*private set;*/ }
        public string SpaceQuotaDefinitionGuid { get { return this.space.SpaceQuotaDefinitionGuid.ToString(); } /*private set;*/ }
    }
}
