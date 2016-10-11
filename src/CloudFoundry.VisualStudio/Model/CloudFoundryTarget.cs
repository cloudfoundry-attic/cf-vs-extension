namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.UAA;
    using CloudFoundry.VisualStudio.Forms;
    using CloudFoundry.VisualStudio.TargetStore;

    internal class CloudFoundryTarget : CloudItem
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
                return this.target.DisplayName;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Cloud;
            }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Remove", Resources.Remove, this.Delete, CloudItemActionContinuation.RefreshParent)
                };
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            CloudFoundryClient client = new CloudFoundryClient(this.target.TargetUrl, this.CancellationToken, null, this.target.IgnoreSSLErrors);

            string password = CloudCredentialsManager.GetPassword(this.target.TargetUrl, this.target.Email);

            var authenticationContext = await client.Login(new CloudCredentials()
            {
                User = this.target.Email,
                Password = password
            });

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

        private async Task Delete()
        {
            var answer = MessageBoxHelper.WarningQuestion(
                string.Format(
                CultureInfo.InvariantCulture,
                "Are you sure you want to delete target '{0}'?",
                this.target.DisplayName));

            if (answer == System.Windows.MessageBoxResult.Yes)
            {
                await Task.Factory.StartNew(() => CloudTargetManager.RemoveTarget(this.target));
            }
        }
    }
}
