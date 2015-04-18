using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
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
        private Uri _targetUri;
        private string _username;
        private string _token;
        private bool _ignoreSslErrors;
        private readonly string _name;

        public CloudFoundryTarget(string name, Uri targetUri, string username, string token, bool ignoreSSLErrors)
            : base(CloudItemType.Target)
        {
            _name = name;
            _targetUri = targetUri;
            _username = username;
            _token = token;
            _ignoreSslErrors = ignoreSSLErrors;
        }

        public Uri TargetUri
        {
            get { return _targetUri; }
            set { _targetUri = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        [Browsable(false)]
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool IgnoreSSLErrors
        {
            get
            {
                return _ignoreSslErrors;
            }
            set
            {
                _ignoreSslErrors = value;
                if (IgnoreSSLErrors)
                {

                }
            }
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this._name, this._targetUri);
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
            CloudFoundryClient client = new CloudFoundryClient(this._targetUri, this.CancellationToken);

            var authenticationContext = await client.Login(this._token);

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
                    new CloudItemAction(this, "Remove", Resources.Remove, () => { return Task.Delay(0); })
                };
            }
        }
    }
}
