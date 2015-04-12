using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class App : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllAppsForSpaceResponse app;

        public App(ListAllAppsForSpaceResponse app, CloudFoundryClient client)
            : base(CloudItemType.App)
        {
            this.client = client;
            this.app = app;
        }

        public override string Text
        {
            get
            {
                return this.app.Name;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                switch (this.app.State.ToUpperInvariant())
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

        public string Buildpack { get {return this.app.Buildpack;} /*private set;*/ }
        public string Command { get { return this.app.Command; } /*private set;*/ }
        public bool Console { get { return this.app.Console; } /*private set;*/ }
        public string Debug { get { return this.app.Debug; } /*private set;*/ }
        public string DetectedBuildpack { get { return this.app.DetectedBuildpack; } /*private set;*/ }
        public string DetectedStartCommand { get { return this.app.DetectedStartCommand; } /*private set;*/ }
        public int? DiskQuota { get { return this.app.DiskQuota; } /*private set;*/ }
        public string DockerImage { get { return this.app.DockerImage; } /*private set;*/ }
        public Metadata EntityMetadata { get { return this.app.EntityMetadata; } /*private set;*/ }
        public string EnvironmentJson { get { return this.app.EnvironmentJson.ToString(); } /*private set;*/ }
        public string EventsUrl { get { return this.app.EventsUrl; } /*private set;*/ }
        public string HealthCheckTimeout { get { return this.app.HealthCheckTimeout; } /*private set;*/ }
        public string HealthCheckType { get { return this.app.HealthCheckType; } /*private set;*/ }
        public int? Instances { get { return this.app.Instances; } /*private set;*/ }
        public int? Memory { get { return this.app.Memory; } /*private set;*/ }
        public string Name { get { return this.app.Name; } /*private set;*/ }
        public string PackageState { get { return this.app.PackageState; } /*private set;*/ }
        public string PackageUpdatedAt { get { return this.app.PackageUpdatedAt; } /*private set;*/ }
        public bool Production { get { return this.app.Production; } /*private set;*/ }
        public string RoutesUrl { get { return this.app.RoutesUrl; } /*private set;*/ }
        public string ServiceBindingsUrl { get { return this.app.ServiceBindingsUrl; } /*private set;*/ }
        public string SpaceGuid { get { return this.app.SpaceGuid.ToString(); } /*private set;*/ }
        public string SpaceUrl { get { return this.app.SpaceUrl; } /*private set;*/ }
        public string StackGuid { get { return this.app.StackGuid.ToString(); } /*private set;*/ }
        public string StackUrl { get { return this.app.StackUrl; } /*private set;*/ }
        public string StagingFailedReason { get { return this.app.StagingFailedReason; } /*private set;*/ }
        public string StagingTaskId { get { return this.app.StagingTaskId; } /*private set;*/ }
        public string State { get { return this.app.State; } /*private set;*/ }
        public string Version { get { return this.app.Version.ToString(); } /*private set;*/ }
    }
}
