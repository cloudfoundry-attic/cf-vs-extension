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

        [Description("URL of the target")]
        [DisplayName("Target URL")]
        public Uri TargetUri
        {
            get { return this.target.TargetUrl; }
        }

        [Description("Username of the Cloud Foundry User")]
        public string Username
        {
            get { return this.target.Email; }
        }

        [Browsable(false)]
        public string Token
        {
            get { return this.target.Token; }
        }

        [Description("API version of the target")]
        public string Version
        {
            get { return this.target.Version; }
        }

        [Description("Indicates if the SSL errors are ignored for the Cloud Foundry Target")]
        [DisplayName("Ignore SSL errors")]
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


            var users = await client.Users.ListAllUsers(new RequestOptions() { Query = string.Format(CultureInfo.InvariantCulture, "username:{0}", this.target.Email) });

            Guid userId = new Guid();

            GetUserSummaryResponse userSummary = null;

            if (users.Properties.TotalResults > 0)
            {
                userId = users[0].EntityMetadata.Guid;

                userSummary = await client.Users.GetUserSummary(userId);
            }

            while (orgs != null && orgs.Properties.TotalResults != 0)
            {
                foreach (var org in orgs)
                {
                    result.Add(new Organization(org, userSummary, client));
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
