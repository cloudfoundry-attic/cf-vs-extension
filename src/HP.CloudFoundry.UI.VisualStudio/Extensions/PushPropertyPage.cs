using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.CloudFoundry.UI.VisualStudio.Extensions
{
    [ComVisible(true), Guid("0f59b9e5-2208-444c-9e4c-d57f698b086d")]
    class PushPropertyPage : Form, IPropertyPage
    {

        //Summary: Return a stucture describing your property page.
        public void GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO info = new PROPPAGEINFO();
            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = "Cloud Foundry";  //Assign tab name
            info.SIZE.cx = this.Size.Width;
            info.SIZE.cy = this.Size.Height;
            if (pPageInfo != null && pPageInfo.Length > 0)
                pPageInfo[0] = info;
        }

        public void Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
        }

        public int Apply()
        {
            return VSConstants.S_OK;
        }

        public new void Deactivate()
        {
        }

        public void Help(string pszHelpDir)
        {
        }

        public int IsPageDirty()
        {
            return false ? (int)VSConstants.S_OK : (int)VSConstants.S_FALSE;
        }

        public new void Move(RECT[] pRect)
        {
        }

        public void SetObjects(uint cObjects, object[] ppunk)
        {
        }

        public void SetPageSite(IPropertyPageSite pPageSite)
        {
        }

        public void Show(uint nCmdShow)
        {
        }

        public int TranslateAccelerator(MSG[] pMsg)
        {
            return 0;
        }
    }
}