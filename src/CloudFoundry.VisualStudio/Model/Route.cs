namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.VisualStudio.Forms;

    internal class Route : CloudItem
    {
        private readonly ListAllRoutesForSpaceResponse route;
        private readonly RetrieveDomainDeprecatedResponse domain;
        private readonly PagedResponseCollection<ListAllAppsForRouteResponse> routeApps;
        private CloudFoundryClient client;

        public Route(ListAllRoutesForSpaceResponse route, RetrieveDomainDeprecatedResponse domain, PagedResponseCollection<ListAllAppsForRouteResponse> routeApps, CloudFoundryClient client)
            : base(CloudItemType.Route)
        {
            this.client = client;
            this.route = route;
            this.domain = domain;
            this.routeApps = routeApps;
        }

        public override string Text
        {
            get
            {
                return this.route.Host;
            }
        }

        [DisplayName("Domain")]
        [Description("Domain of the route.")]
        public string Domain
        {
            get
            {
                return this.domain.Name;
            }
        }

        [DisplayName("Bound applications")]
        [Description("Applications that have the current route bound to.")]
        public string Apps
        {
            get
            {
                var appNames = string.Empty;
                foreach (var app in this.routeApps)
                {
                    appNames = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", app.Name, appNames);
                }

                return appNames.Trim().TrimEnd(',');
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Route;
            }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Delete", Resources.Delete, this.Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }

        private async Task Delete()
        {
            try
            {
                this.EnableNodes(this.route.EntityMetadata.Guid, false);
                var answer = MessageBoxHelper.WarningQuestion(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "Are you sure you want to delete route '{0}'?",
                    this.route.Host));

                if (answer == System.Windows.Forms.DialogResult.Yes)
                {
                    await this.client.Routes.DeleteRoute(this.route.EntityMetadata.Guid);
                }
            }
            finally
            {
                this.EnableNodes(this.route.EntityMetadata.Guid, true);
            }
        }
    }
}
