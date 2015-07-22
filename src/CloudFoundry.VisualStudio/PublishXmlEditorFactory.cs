namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Globalization;
    using System.IO;
    using CloudFoundry.VisualStudio.Forms;
    using CloudFoundry.VisualStudio.ProjectPush;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class PublishXmlEditorFactory : IVsEditorFactory
    {
        public int Close()
        {
            return VSConstants.S_OK;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Prevent VS crash")]
        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy ppvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            ppunkDocData = IntPtr.Zero;
            ppunkDocView = IntPtr.Zero;
            pguidCmdUI = new Guid("41d526d3-6281-42ff-ba9f-e5746623233f");
            pgrfCDW = 0;
            pbstrEditorCaption = "Cloud Foundry Publish Profile";

            var project = VsUtils.GetSelectedProject();

            var fileInfo = new FileInfo(pszMkDocument);

            if (project == null)
            {
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }

            if (project.Name.Contains("Miscellaneous Files"))
            {
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }

            if (!fileInfo.Name.ToUpperInvariant().EndsWith(PushEnvironment.Extension, StringComparison.OrdinalIgnoreCase))
            {
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }

            PublishProfile packageFile;
            try
            {
                PushEnvironment environment = new PushEnvironment();
                environment.ProjectName = project.Name;
                environment.ProfileFilePath = pszMkDocument;

                packageFile = PublishProfile.Load(environment);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayError(string.Format(CultureInfo.InvariantCulture, "Cannot load {0}. {1}.", pszMkDocument, ex.Message));
                Logger.Error("Exception loading package file", ex);
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            var dialog = new PushDialog(packageFile);
            dialog.ShowModal();
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = string.Empty;
            return VSConstants.S_OK;
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            return VSConstants.S_OK;
        }
    }
}