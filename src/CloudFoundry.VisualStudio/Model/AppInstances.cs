namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    
    internal class AppInstances : CloudItem
    {
        private readonly GetAppSummaryResponse app;
        private readonly int instanceNumber;
        private CloudFoundryClient client;

        public AppInstances(GetAppSummaryResponse app, int instanceNumber, CloudFoundryClient client) : base(CloudItemType.InstancesCollection)
        {
            this.app = app;
            this.instanceNumber = instanceNumber;
            this.client = client;
        }

        public override string Text
        {
            get 
            {
                return "Instance " + this.instanceNumber;
            }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get { return Resources.Browse; }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get { return null; }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<CloudItem> files = new List<CloudItem>();

            List<RetrieveFileResponse> fileList = await this.client.Files.RetrieveFile(this.app.Guid, this.instanceNumber, string.Empty);

            foreach (RetrieveFileResponse fileItem in fileList)
            {
                if (fileItem.FileSize == "-")
                {
                    AppFolder fileInfo = new AppFolder(fileItem.FileName, fileItem.FileName, this.instanceNumber, this.app, this.client);
                    files.Add(fileInfo);
                }
                else
                {
                    AppFile fileInfo = new AppFile(fileItem.FileName, fileItem.FileName, this.instanceNumber, this.app, this.client);
                    files.Add(fileInfo);
                }
            }

            return files;
        }
    }
}
