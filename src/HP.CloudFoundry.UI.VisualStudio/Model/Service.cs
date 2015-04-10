using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class Service : CloudItem
    {
        private CloudFoundryClient client;
        private ListAllServiceInstancesForSpaceResponse service;

        public Service(ListAllServiceInstancesForSpaceResponse service, CloudFoundryClient client)
            : base(CloudItemType.ServicesCollection)
        {
            this.client = client;
            this.service = service;
        }

        public override string Text
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.service.Name, this.service.Type);
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get
            {
                return Resources.Service;
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
