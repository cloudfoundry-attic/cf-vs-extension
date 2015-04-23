using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using CloudFoundry.VisualStudio.Forms;
using System.ComponentModel;

namespace CloudFoundry.VisualStudio.Model
{
    class App : CloudItem
    {
        private CloudFoundryClient _client;
        private readonly GetAppSummaryResponse _app;

        public App(GetAppSummaryResponse app, CloudFoundryClient client)
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
                        if (_app.RunningInstances > 0)
                        {
                            return Resources.StatusRunning;
                        }
                        else
                        {
                            return Resources.StatusStarted;
                        }
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
                    new CloudItemAction(this, "Start", Resources.Start, Start, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Restart", Resources.Restart, Restart, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Stop", Resources.Stop, Stop, CloudItemActionContinuation.RefreshParent),
                    new CloudItemAction(this, "Delete", Resources.Delete, Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }

        private async Task Stop()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STOPPED"
            };

            await this._client.Apps.UpdateApp(_app.Guid, updateAppRequest);
        }

        private async Task Start()
        {
            var updateAppRequest = new UpdateAppRequest()
            {
                State = "STARTED"
            };

            await this._client.Apps.UpdateApp(_app.Guid, updateAppRequest);
        }

        private async Task Browse()
        {
            if (this._app.Routes == null)
            {
                return;
            }

            var route = _app.Routes.FirstOrDefault();

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
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete application '{0}'? By deleting this application, all it's service bindings will be deleted.",
                this._app.Name
                ));

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                var serviceBindings = await _client.Apps.ListAllServiceBindingsForApp(this._app.Guid);

                foreach (var serviceBinding in serviceBindings)
                {
                    await this._client.ServiceBindings.DeleteServiceBinding(serviceBinding.EntityMetadata.Guid);
                }

                await this._client.Apps.DeleteApp(this._app.Guid);
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

        [DisplayName("Detected buildpack")]
        [Description("The buildpack that was detected by the Cloud Constroller.")]
        public string DetectedBuildpack { get { return this._app.DetectedBuildpack; } /*private set;*/ }

        [Description("Number of instances that the application has.")]
        public int? Instances { get { return this._app.Instances; } /*private set;*/ }

        [Description("Number of running instances.")]
        public int? RunningInstances { get { return this._app.RunningInstances; } /*private set;*/ }

        [DisplayName("Maximum memory")]
        [Description("Maximum memory of the application.")]
        public int? Memory { get { return this._app.Memory; } /*private set;*/ }

        [Description("The name of the application.")]
        public string Name { get { return this._app.Name; } /*private set;*/ }

        [DisplayName("Last package update")]
        [Description("Date when the application's package was last updated.")]
        public string CreationDate { get { return this._app.PackageUpdatedAt; } }
    }
}
