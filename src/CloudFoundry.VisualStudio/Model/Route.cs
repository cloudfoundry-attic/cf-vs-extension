using System.Globalization;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CloudFoundry.VisualStudio.Forms;
using System.ComponentModel;

namespace CloudFoundry.VisualStudio.Model
{
    class Route : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly ListAllRoutesForSpaceResponse _route;
        private readonly RetrieveDomainDeprecatedResponse _domain;
        private readonly PagedResponseCollection<ListAllAppsForRouteResponse> _routeApps;

        public Route(ListAllRoutesForSpaceResponse route, RetrieveDomainDeprecatedResponse domain, PagedResponseCollection<ListAllAppsForRouteResponse> routeApps, CloudFoundryClient client)
            : base(CloudItemType.Route)
        {
            _client = client;
            _route = route;
            _domain = domain;
            _routeApps = routeApps;
        }

        public override string Text
        {
            get
            {
                return _route.Host;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Route;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<CloudItem[]>.Run(() =>
            {
                return new CloudItem[] { };
            });
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Delete", Resources.Delete, Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }


        private async Task Delete()
        {
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete route '{0}'?",
                this._route.Host
                ));

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                await this._client.Routes.DeleteRoute(this._route.EntityMetadata.Guid);
            }
        }

        [DisplayName("Domain")]
        [Description("Domain of the route.")]
        public string Domain { get { return _domain.Name; } }

        [DisplayName("Bound applications")]
        [Description("Applications that have the current route bound to.")]
        public string Apps
        {
            get
            {
                var appNames = string.Empty;
                foreach (var app in _routeApps)
                {
                    appNames = string.Format("{0}, {1}", app.Name, appNames);
                }

                return appNames.Trim().TrimEnd(',');
            }
        }
    }
}
