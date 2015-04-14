using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace HP.CloudFoundry.UI.VisualStudio.Extensions
{
    [ComVisible(true)]
    [Guid(CustomPropertyPageProjectFactoryGuidString)]
    public class Factory : FlavoredProjectFactoryBase
    {
        public const string CustomPropertyPageProjectFactoryGuidString =
    "FAE04EC0-301F-11D3-BF4B-00C04F79EFBB";
        public static readonly Guid CustomPropertyPageProjectFactoryGuid =
            new Guid(CustomPropertyPageProjectFactoryGuidString);


        private readonly Package package;

        public Factory(Package package)
        {
            this.package = package;
            //ErrorHandler.ThrowOnFailure(GlobalServices.Solution.AdviseSolutionEvents(this, out solutionCookie));
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            var project = new ProjectManager();
            project.SetSite((IOleServiceProvider)((IServiceProvider)package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }

    /// <summary>
    /// Represents the root node of the extended project node tree
    /// </summary>
    [ComVisible(true)]
    public class ProjectManager : FlavoredProjectBase
    {
        public ProjectManager()
            : base()
        { }

        /// <summary>
        /// Sets the service provider from which to access the services. 
        /// </summary>
        /// <param name="site">An instance to an Microsoft.VisualStudio.OLE.Interop object</param>
        /// <returns>A success or failure value.</returns>
        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
        {
            serviceProvider = new ServiceProvider(site);
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Modify how properties are calculated
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="propId"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            int result = base.GetProperty(itemId, propId, out property);
            if (result != VSConstants.S_OK)
                return result;

            // Adjust the results returned by the base project
            if (itemId == VSConstants.VSITEMID_ROOT)
            {
                switch ((__VSHPROPID2)propId)
                {
                    case __VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                        {
                            //Add the CompileOrder property page.
                            var properties = new List<string>(property.ToString().Split(';'));
                            properties.Add(typeof(PushPropertyPage).GUID.ToString("B"));
                            property = properties.Aggregate("", (a, next) => a + ';' + next).Substring(1);
                            return VSConstants.S_OK;
                        }
                    case __VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList:
                        {
                            // set the order for the project property pages
                            var properties = new List<string>(property.ToString().Split(';'));
                            properties.Insert(1, typeof(PushPropertyPage).GUID.ToString("B"));
                            property = properties.Aggregate("", (a, next) => a + ';' + next).Substring(1);
                            return VSConstants.S_OK;
                        }
                    default:
                        break;
                }
            }
            return result;
        }
    }

    [ComVisible(true), Guid("0f59b9e5-2208-444c-9e4c-d57f698b086d")]
    public class PushPropertyPage : IPropertyPage
    {
        Panel control;
        bool dirty = false;
        uint eventCookie;

        protected bool IsDirty
        {
            get
            {
                return this.dirty;
            }
            set
            {
                if (this.dirty != value)
                {
                    this.dirty = value;
                    if (this.site != null)
                        site.OnStatusChange((uint)(this.dirty ? StructuresEnums.PropPageStatus.Dirty : StructuresEnums.PropPageStatus.Clean));
                }
            }
        }

        #region IPropertyPage Members

        public void Activate(IntPtr parent, RECT[] pRect, int bModal)
        {
            if (this.control == null)
            {
                this.control = new Panel();
                this.control.Size = new Size(pRect[0].right - pRect[0].left, pRect[0].bottom - pRect[0].top);
                this.control.Visible = false;
                this.control.Size = new Size(550, 300);
                this.control.CreateControl();
                NativeMethods.SetParent(this.control.Handle, parent);
                //this.control.OnPageUpdated += (sender, args) => IsDirty = true;
            }
        }

        public int Apply()
        {
            IsDirty = false;
            return VSConstants.S_OK;
        }

        public void Deactivate()
        {
            if (null != this.control)
            {
                this.control.Dispose();
                this.control = null;
            }
        }

        public void GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = "Cloud Foundry";
            info.SIZE.cx = 550;
            info.SIZE.cy = 300;
            pPageInfo[0] = info;
        }

        public void Help(string pszHelpDir)
        {
        }

        public int IsPageDirty()
        {
            // Note this returns an HRESULT not a Bool.
            return (dirty ? (int)VSConstants.S_OK : (int)VSConstants.S_FALSE);
        }

        public void Move(RECT[] pRect)
        {
            RECT r = pRect[0];
            this.control.Location = new Point(r.left, r.top);
            this.control.Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        public void SetObjects(uint count, object[] ppunk)
        {
           
        }

        IPropertyPageSite site;
        public void SetPageSite(IPropertyPageSite pPageSite)
        {
            this.site = pPageSite;
        }

        public void Show(uint nCmdShow)
        {
            this.control.Visible = true; // TODO: pass SW_SHOW* flags through      
            this.control.Show();
        }

        public int TranslateAccelerator(MSG[] pMsg)
        {
            MSG msg = pMsg[0];

            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) && (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST))
                return 1;

            return (NativeMethods.IsDialogMessageA(this.control.Handle, ref msg)) ? 0 : 1;
        }

        #endregion
    }
}