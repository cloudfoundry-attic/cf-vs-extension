namespace CloudFoundry.VisualStudio.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.VisualStudio.Forms;

    internal class App : CloudItem
    {
        private readonly GetAppSummaryResponse app;
        private CloudFoundryClient client;

        public App(GetAppSummaryResponse app, CloudFoundryClient client)
            : base(CloudItemType.App)
        {
            this.client = client;
            this.app = app;
        }

        [Description("The buildpack of the application.")]
        public string Buildpack
        {
            get { return this.app.Buildpack; }
        }

        [DisplayName("Detected buildpack")]
        [Description("The buildpack that was detected by the Cloud Controller.")]
        public string DetectedBuildpack
        {
            get { return this.app.DetectedBuildpack; }
        }

        [Description("Number of instances that the application has.")]
        public int? Instances
        {
            get { return this.app.Instances; }
        }

        [Description("Number of running instances.")]
        public int? RunningInstances
        {
            get { return this.app.RunningInstances; }
        }

        [DisplayName("Maximum memory")]
        [Description("Maximum memory of the application.")]
        public int? Memory
        {
            get { return this.app.Memory; }
        }

        [Description("The name of the application.")]
        public string Name
        {
            get { return this.app.Name; }
        }

        [DisplayName("Last package update")]
        [Description("Date when the application's package was last updated.")]
        public string CreationDate
        {
            get { return this.app.PackageUpdatedAt; }
        }

        public override string Text
        {
            get { return this.app.Name; }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                if (this.app == null)
                {
                    return Resources.StatusUnknown;
                }

                switch (this.app.State.ToUpperInvariant())
                {
                    case "STARTED":
                        if (this.app.RunningInstances > 0)
                        {
                            return Resources.AppRunning;
                        }
                        else
                        {
                            return Resources.AppStarted;
                        }

                    case "STOPPED":
                        return Resources.AppStopped;
                    case "RUNNING":
                        return Resources.AppRunning;
                    default:
                        return Resources.AppUnknown;
                }
            }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "View in Browser", Resources.Browse, this.Browse),
                    new CloudItemAction(this, "Start", Resources.Start, this.Start, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Restart", Resources.Restart, this.Restart, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Stop", Resources.Stop, this.Stop, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Delete", Resources.Delete, this.Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            return await Task<List<AppInstances>>.Run(() =>
            {
                List<AppInstances> instancesList = new List<AppInstances>();

                for (int i = 0; i < this.app.RunningInstances; i++)
                {
                    AppInstances item = new AppInstances(this.app, i, client);
                    instancesList.Add(item);
                }

                return instancesList;
            });
        }

        private async Task Stop()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STOPPED"
            };

            await this.client.Apps.UpdateApp(this.app.Guid, updateAppRequest);
        }

        private async Task Start()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STARTED"
            };

            await this.client.Apps.UpdateApp(this.app.Guid, updateAppRequest);
        }

        private async Task Browse()
        {
            if (this.app.Routes == null)
            {
                return;
            }

            var route = this.app.Routes.FirstOrDefault();

            if (route == null)
            {
                return;
            }

            var host = route["host"];
            var domain = route["domain"];

            if (domain == null)
            {
                return;
            }

            var domainName = domain["name"];

            var url = string.Format(CultureInfo.InvariantCulture, "http://{0}.{1}", host, domainName);

            await Task.Run(() =>
            {
                Process.Start(url);
            });
        }

        private async Task Delete()
        {
            try
            {
                this.EnableNodes(this.app.Guid, false);

                var answer = MessageBoxHelper.WarningQuestion(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "Are you sure you want to delete application '{0}'? By deleting this application, all it's service bindings will be deleted.",
                    this.app.Name));

                if (answer == System.Windows.Forms.DialogResult.Yes)
                {
                    var serviceBindings = await this.client.Apps.ListAllServiceBindingsForApp(this.app.Guid);

                    foreach (var serviceBinding in serviceBindings)
                    {
                        await this.client.ServiceBindings.DeleteServiceBinding(serviceBinding.EntityMetadata.Guid);
                    }

                    await this.client.Apps.DeleteApp(this.app.Guid);
                }
            }
            finally
            {
                this.EnableNodes(this.app.Guid, true);
            }
        }

        private async Task Restart()
        {
            await this.Stop().ContinueWith(async (antecedent) =>
            {
                await Start();
            });
        }
    }
}
