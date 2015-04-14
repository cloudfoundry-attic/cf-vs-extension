using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.CloudFoundry.UI.VisualStudio.Extensions
{
  
    [ComVisible(true)]
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
            return 0;
        }

        public new void Deactivate()
        {
        }

        public void Help(string pszHelpDir)
        {
        }

        public int IsPageDirty()
        {
            return 0;
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