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

    internal class AppFile : CloudItem
    {
        private readonly string fileName;
        private readonly string filePath;
        private readonly CloudFoundryClient client;
        private readonly GetAppSummaryResponse app;
        private readonly int instanceNumber;

        public AppFile(string fileName, string filePath, int instanceNumber, GetAppSummaryResponse app, CloudFoundryClient client)
            : base(CloudItemType.AppFile)
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
            get { return GetFileIcon(this.fileName, false, false).ToBitmap(); }
        }

        protected override IEnumerable<CloudItemAction> MenuActions
        {
            get
            {
                return new CloudItemAction[]
                {
                    new CloudItemAction(this, "Download file", Resources.Synchronizing, this.DownloadFile),
                };
            }
        }

        protected override async Task<IEnumerable<CloudItem>> UpdateChildren()
        {
            await this.DownloadFile();
            return await Task<CloudItem[]>.Run(() => { return new CloudItem[] { }; });
        }

        private static System.Drawing.Icon GetFileIcon(string name, bool getLargeIcon, bool linkOverlay)
        {
            NativeMethods.SHFILEINFO shfi = new NativeMethods.SHFILEINFO();
            uint flags = NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_USEFILEATTRIBUTES;

            if (true == linkOverlay)
            {
                flags += NativeMethods.SHGFI_LINKOVERLAY;
            }

            /* Check the size specified for return. */
            if (getLargeIcon == false)
            {
                flags += NativeMethods.SHGFI_SMALLICON; // include the small icon flag
            }
            else
            {
                flags += NativeMethods.SHGFI_LARGEICON;  // include the large icon flag
            }

            NativeMethods.SHGetFileInfo(name, NativeMethods.FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), flags);

            // Copy (clone) the returned icon to a new object, thus allowing us 
            // to call DestroyIcon immediately
            System.Drawing.Icon icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();
            NativeMethods.DestroyIcon(shfi.hIcon); // Cleanup

            return icon;
        }

        private async Task DownloadFile()
        {
            List<RetrieveFileResponse> file = await this.client.Files.RetrieveFile(this.app.Guid, this.instanceNumber, this.filePath);

            if (file.Count == 1)
            {
                string content = file[0].FileContent;

                string downloadPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), this.fileName);

                System.IO.File.WriteAllText(downloadPath, content);

                EnvDTE.DTE dte = (EnvDTE.DTE)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(EnvDTE.DTE));
                dte.ItemOperations.OpenFile(downloadPath);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error retrieving file contents {0}", this.fileName));
            }
        }
    }
}
