using CloudFoundry.VisualStudio.Forms;
using CloudFoundry.VisualStudio.ProjectPush;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio
{
    class PublishXmlEditorFactory : IVsEditorFactory
    {
        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {

            AppPackage packageFile = new AppPackage();
            packageFile.LoadFromFile(pszMkDocument);

            var dialog = new EditDialog(packageFile, null, false);
            dialog.ShowDialog();

            ppunkDocData = IntPtr.Zero;
            ppunkDocView = IntPtr.Zero;
            pguidCmdUI = new Guid("41d526d3-6281-42ff-ba9f-e5746623233f");
            pgrfCDW = 0;
            pbstrEditorCaption = "Cloud Foundry Publish Profile";

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