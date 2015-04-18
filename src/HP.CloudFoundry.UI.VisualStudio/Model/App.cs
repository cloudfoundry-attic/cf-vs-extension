using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using HP.CloudFoundry.UI.VisualStudio.Forms;
using System.ComponentModel;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class App : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly ListAllAppsForSpaceResponse _app;

        public App(ListAllAppsForSpaceResponse app, CloudFoundryClient client)
            : base(CloudItemType.App)
        {
            _client = client;
            _app = app;
        }

        public override string Text
        {
            get
            {
                return _app.Name;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                if (_app == null) return Resources.StatusUnknown;
                switch (_app.State.ToUpperInvariant())
                {
                    case "STARTED":
                        return Resources.StatusStarted;
                    case "STOPPED":
                        return Resources.StatusStopped;
                    case "RUNNING":
                        return Resources.StatusRunning;
                    default:
                        return Resources.StatusUnknown;
                }
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
                    new CloudItemAction(this, "Browse", Resources.Browse, Browse),
                    new CloudItemAction(this, "Start", Resources.Start, Start),
                    new CloudItemAction(this, "Restart", Resources.Restart, Restart),
                    new CloudItemAction(this, "Stop", Resources.Stop, Stop),
                    new CloudItemAction(this, "Delete", Resources.Delete, Delete)
                };
            }
        }

        private async Task Stop()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STOPPED"
            };

            await this._client.Apps.UpdateApp(_app.EntityMetadata.Guid, updateAppRequest);
        }

        private async Task Start()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STARTED"
            };

            await this._client.Apps.UpdateApp(_app.EntityMetadata.Guid, updateAppRequest);
        }

        private async Task Browse()
        {
            var summary = await this._client.Apps.GetAppSummary(this._app.EntityMetadata.Guid);

            if (summary.Routes == null)
            {
                return;
            }

            var route = summary.Routes.FirstOrDefault();

            if (route == null)
            {
                return;
            }

            var host = route["host"];
            var domain = route["domain"]["name"];


            var url = string.Format(CultureInfo.InvariantCulture, "http://{0}.{1}", host, domain);

            Process.Start(url);
        }

        private async Task Delete()
        {
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete application '{0}'?",
                this._app.Name
                ));

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                await this._client.Apps.DeleteApp(this._app.EntityMetadata.Guid);
            }
        }

        private async Task Restart()
        {
            await this.Stop().ContinueWith(async (antecedent) =>
            {
                await Start();
            });
        }


        [Description("The buildpack of the application.")]
        public string Buildpack { get { return this._app.Buildpack; } /*private set;*/ }

        [Browsable(false)]
        public string Command { get { return this._app.Command; } /*private set;*/ }

        [Browsable(false)]
        public bool Console { get { return this._app.Console; } /*private set;*/ }

        [Browsable(false)]
        public string Debug { get { return this._app.Debug; } /*private set;*/ }

        [DisplayName("Detected buildpack")]
        [Description("The buildpack that was detected by the Cloud Constroller.")]
        public string DetectedBuildpack { get { return this._app.DetectedBuildpack; } /*private set;*/ }

        [Browsable(false)]
        public string DetectedStartCommand { get { return this._app.DetectedStartCommand; } /*private set;*/ }

        [Browsable(false)]
        public int? DiskQuota { get { return this._app.DiskQuota; } /*private set;*/ }

        [Browsable(false)]
        public string DockerImage { get { return this._app.DockerImage; } /*private set;*/ }

        [Browsable(false)]
        public Metadata EntityMetadata { get { return this._app.EntityMetadata; } /*private set;*/ }

        [Browsable(false)]
        public string EnvironmentJson { get { return this._app.EnvironmentJson.ToString(); } /*private set;*/ }

        [Browsable(false)]
        public string EventsUrl { get { return this._app.EventsUrl; } /*private set;*/ }

        [Browsable(false)]
        public string HealthCheckTimeout { get { return this._app.HealthCheckTimeout; } /*private set;*/ }

        [Browsable(false)]
        public string HealthCheckType { get { return this._app.HealthCheckType; } /*private set;*/ }

        [Description("Number of instances that the application has.")]
        public int? Instances { get { return this._app.Instances; } /*private set;*/ }

        [DisplayName("Maximum memory")]
        [Description("Maximum memory of the application.")]
        public int? Memory { get { return this._app.Memory; } /*private set;*/ }

        [Description("The name of the application.")]
        public string Name { get { return this._app.Name; } /*private set;*/ }

        [Browsable(false)]
        public string PackageState { get { return this._app.PackageState; } /*private set;*/ }

        [Browsable(false)]
        public string PackageUpdatedAt { get { return this._app.PackageUpdatedAt; } /*private set;*/ }

        [Browsable(false)]
        public bool Production { get { return this._app.Production; } /*private set;*/ }

        [Browsable(false)]
        public string RoutesUrl { get { return this._app.RoutesUrl; } /*private set;*/ }

        [Browsable(false)]
        public string ServiceBindingsUrl { get { return this._app.ServiceBindingsUrl; } /*private set;*/ }

        [Browsable(false)]
        public string SpaceGuid { get { return this._app.SpaceGuid.ToString(); } /*private set;*/ }

        [Browsable(false)]
        public string SpaceUrl { get { return this._app.SpaceUrl; } /*private set;*/ }

        [Browsable(false)]
        public string StackGuid { get { return this._app.StackGuid.ToString(); } /*private set;*/ }

        [Browsable(false)]
        public string StackUrl { get { return this._app.StackUrl; } /*private set;*/ }

        [Browsable(false)]
        public string StagingFailedReason { get { return this._app.StagingFailedReason; } /*private set;*/ }

        [Browsable(false)]
        public string StagingTaskId { get { return this._app.StagingTaskId; } /*private set;*/ }

        [Browsable(false)]
        public string State { get { return this._app.State; } /*private set;*/ }

        [Browsable(false)]
        public string Version { get { return this._app.Version.ToString(); } /*private set;*/ }

        [DisplayName("Creation date")]
        [Description("Date when the application was created.")]
        public string CreationDate { get { return this._app.EntityMetadata.CreatedAt; } }
    }
}
