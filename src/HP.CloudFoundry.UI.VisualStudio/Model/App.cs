using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
                return new CloudItem[] {};
            });
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction("Start", Resources.StatusStopped, () => {}),
                    new CloudItemAction("Restart", Resources.StatusStopped, () => {}),
                    new CloudItemAction("Stop", Resources.StatusStopped, () => {}),
                    new CloudItemAction("Delete", Resources.StatusStopped, () => {})
                };
            }
        }

        public string Buildpack { get {return this._app.Buildpack;} /*private set;*/ }
        public string Command { get { return this._app.Command; } /*private set;*/ }
        public bool Console { get { return this._app.Console; } /*private set;*/ }
        public string Debug { get { return this._app.Debug; } /*private set;*/ }
        public string DetectedBuildpack { get { return this._app.DetectedBuildpack; } /*private set;*/ }
        public string DetectedStartCommand { get { return this._app.DetectedStartCommand; } /*private set;*/ }
        public int? DiskQuota { get { return this._app.DiskQuota; } /*private set;*/ }
        public string DockerImage { get { return this._app.DockerImage; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this._app.EntityMetadata; } /*private set;*/ }
        public string EnvironmentJson { get { return this._app.EnvironmentJson.ToString(); } /*private set;*/ }
        public string EventsUrl { get { return this._app.EventsUrl; } /*private set;*/ }
        public string HealthCheckTimeout { get { return this._app.HealthCheckTimeout; } /*private set;*/ }
        public string HealthCheckType { get { return this._app.HealthCheckType; } /*private set;*/ }
        public int? Instances { get { return this._app.Instances; } /*private set;*/ }
        public int? Memory { get { return this._app.Memory; } /*private set;*/ }
        public string Name { get { return this._app.Name; } /*private set;*/ }
        public string PackageState { get { return this._app.PackageState; } /*private set;*/ }
        public string PackageUpdatedAt { get { return this._app.PackageUpdatedAt; } /*private set;*/ }
        public bool Production { get { return this._app.Production; } /*private set;*/ }
        public string RoutesUrl { get { return this._app.RoutesUrl; } /*private set;*/ }
        public string ServiceBindingsUrl { get { return this._app.ServiceBindingsUrl; } /*private set;*/ }
        public string SpaceGuid { get { return this._app.SpaceGuid.ToString(); } /*private set;*/ }
        public string SpaceUrl { get { return this._app.SpaceUrl; } /*private set;*/ }
        public string StackGuid { get { return this._app.StackGuid.ToString(); } /*private set;*/ }
        public string StackUrl { get { return this._app.StackUrl; } /*private set;*/ }
        public string StagingFailedReason { get { return this._app.StagingFailedReason; } /*private set;*/ }
        public string StagingTaskId { get { return this._app.StagingTaskId; } /*private set;*/ }
        public string State { get { return this._app.State; } /*private set;*/ }
        public string Version { get { return this._app.Version.ToString(); } /*private set;*/ }
    }
}
