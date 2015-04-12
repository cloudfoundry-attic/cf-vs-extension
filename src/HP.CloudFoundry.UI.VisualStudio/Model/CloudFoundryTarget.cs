using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class CloudFoundryTarget : CloudItem
    {
        private Uri targetUri;
        private string username;
        private string password;
        private bool ignoreSSLErrors;
        private string name;

        public CloudFoundryTarget(string name, Uri targetUri, string username, string password, bool ignoreSSLErrors)
            : base(CloudItemType.Target)
        {
            this.name = name;
            this.targetUri = targetUri;
            this.username = username;
            this.password = password;
            this.ignoreSSLErrors = ignoreSSLErrors;
        }

        public Uri TargetUri
        {
            get { return targetUri; }
            set { targetUri = value; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        [Browsable(false)]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public bool IgnoreSSLErrors
        {
            get 
            { 
                return this.ignoreSSLErrors; 
            }
            set 
            { 
                this.ignoreSSLErrors = value; 
                if (this.IgnoreSSLErrors)
                {
                   
                }
            }
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.name, this.targetUri);
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
            CloudFoundryClient client = new CloudFoundryClient(this.targetUri, this.CancellationToken);

            var authenticationContext = await client.Login(new CloudCredentials()
            {
                User = this.username,
                Password = this.password
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
    }
}
