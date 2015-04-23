using CloudFoundry.VisualStudio.ProjectPush;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio
{
    internal class RunningDocTableEvents : IVsRunningDocTableEvents3
    {
        private HP_CloudFoundry_UI_VisualStudioPackage package;

        public RunningDocTableEvents(HP_CloudFoundry_UI_VisualStudioPackage package)
        {
            this.package = package;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {            
            RunningDocumentInfo runningDocumentInfo = package.rdt.Value.GetDocumentInfo(docCookie);
            string documentPath = runningDocumentInfo.Moniker;

            if (fFirstShow == 1 && documentPath.Contains("cf.pubxml"))
            {
                AppPackage packageFile = new AppPackage();
                packageFile.LoadFromFile(documentPath);

                var dialog = new EditDialog(packageFile, false);
                dialog.ShowDialog();
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }
    }
}
