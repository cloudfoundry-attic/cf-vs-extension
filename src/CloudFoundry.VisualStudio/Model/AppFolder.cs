namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    
    internal class AppFolder : CloudItem
    {
        private readonly string fileName;
        private readonly string filePath;
        private readonly CloudFoundryClient client;
        private readonly GetAppSummaryResponse app;
        private readonly int instanceNumber;

        public AppFolder(string fileName, string filePath, int instanceNumber, GetAppSummaryResponse app, CloudFoundryClient client) : base(CloudItemType.AppFolder)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.client = client;
            this.app = app;
            this.instanceNumber = instanceNumber;
        }

        public override string Text
        {
            get { return this.fileName; }
        }

        protected override System.Drawing.Bitmap IconBitmap
        {
            get { return Resources.Apps; }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                    return null;
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            List<CloudItem> files = new List<CloudItem>();

            List<RetrieveFileResponse> fileList = await this.client.Files.RetrieveFile(this.app.Guid, this.instanceNumber, this.filePath);

            foreach (RetrieveFileResponse fileItem in fileList)
            {
                if (fileItem.FileSize == "-")
                {
                    AppFolder fileInfo = new AppFolder(fileItem.FileName, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.filePath, fileItem.FileName), this.instanceNumber, this.app, this.client);
                    files.Add(fileInfo);
                }
                else
                {
                    AppFile fileInfo = new AppFile(fileItem.FileName, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.filePath, fileItem.FileName), this.instanceNumber, this.app, this.client);
                    files.Add(fileInfo);
                }
            }

            return files;
        }          
    }
}
