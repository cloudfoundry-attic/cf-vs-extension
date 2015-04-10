using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    }
}
