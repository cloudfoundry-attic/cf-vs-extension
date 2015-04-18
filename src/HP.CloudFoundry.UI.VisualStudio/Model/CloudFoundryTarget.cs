using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using HP.CloudFoundry.UI.VisualStudio.Forms;
using HP.CloudFoundry.UI.VisualStudio.TargetStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class CloudFoundryTarget : CloudItem
    {
        private readonly CloudTarget target;

        public CloudFoundryTarget(CloudTarget target)
            : base(CloudItemType.Target)
        {
            this.target = target;
        }

        public Uri TargetUri
        {
            get { return this.target.TargetUrl; }
        }

        public string Username
        {
            get { return this.target.Email; }
        }

        [Browsable(false)]
        public string Token
        {
            get { return this.target.Token; }
        }

        public bool IgnoreSSLErrors
        {
            get
            {
                return this.target.IgnoreSSLErrors;
            }
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.target.DisplayName, this.target.TargetUrl);
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Cloud;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            CloudFoundryClient client = new CloudFoundryClient(this.target.TargetUrl, this.CancellationToken);

            var authenticationContext = await client.Login(this.target.Token);

            List<Organization> result = new List<Organization>();

            PagedResponseCollection<ListAllOrganizationsResponse> orgs = await client.Organizations.ListAllOrganizations();

            while (orgs != null && orgs.Properties.TotalResults != 0)
            {
                foreach (var org in orgs)
                {
                    result.Add(new Organization(org, client));
                }

                orgs = await orgs.GetNextPage();
            }

            return result;
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Remove", Resources.Remove, Delete)
                };
            }
        }

        private async Task Delete()
        {
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete target '{0}'?",
                this.target.DisplayName
                ));

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                await Task.Factory.StartNew(() => CloudTargetManager.RemoveTarget(this.target));
            }
        }
    }
}
