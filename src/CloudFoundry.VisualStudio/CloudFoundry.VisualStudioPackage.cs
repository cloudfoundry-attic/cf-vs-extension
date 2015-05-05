using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using EnvDTE;
using System.Threading.Tasks;
using System.IO;
using NuGet.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using CloudFoundry.VisualStudio.Model;
using CloudFoundry.VisualStudio.ProjectPush;
using CloudFoundry.VisualStudio.Forms;

namespace CloudFoundry.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(CloudFoundryExplorerToolWindow))]
    [Guid(GuidList.guidCloudFoundry_VisualStudioPkgString)]

    [ProvideEditorFactory(typeof(PublishXmlEditorFactory), 101)]
    [ProvideEditorExtension(typeof(PublishXmlEditorFactory), ".pubxml", 100)]
    // Our Editor supports Find and Replace therefore we need to declare support for LOGVIEWID_TextView.
    // This attribute declares that your EditorPane class implements IVsCodeWindow interface
    // used to navigate to find results from a "Find in Files" type of operation.
    [ProvideEditorLogicalView(typeof(PublishXmlEditorFactory), VSConstants.LOGVIEWID.TextView_string)]

    public sealed class CloudFoundry_VisualStudioPackage : Package
    {
        public const string packageId = "cf-msbuild-tasks";

        public const string extension = ".cf.pubxml";


        List<int> dynamicExtenderProviderCookies = new List<int>();

        DTE dte;
        internal readonly Lazy<RunningDocumentTable> rdt;
        internal readonly Lazy<Microsoft.VisualStudio.OLE.Interop.IServiceProvider> sp;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CloudFoundry_VisualStudioPackage()
        {
            dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

            sp = new Lazy<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>(() => Package.GetGlobalService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            rdt = new Lazy<RunningDocumentTable>(() => new RunningDocumentTable(new ServiceProvider(sp.Value)));

            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(CloudFoundryExplorerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            base.RegisterEditorFactory(new PublishXmlEditorFactory());

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidCloudFoundry_VisualStudioCmdSet, (int)PkgCmdIDList.cmdidCloudFoundryExplorer);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);

                // Create the command for button ButtonBuildAndPushProject
                CommandID commandId = new CommandID(GuidList.guidCloudFoundry_VisualStudioCmdSet, (int)PkgCmdIDList.cmdidButtonPublishProject);
                OleMenuCommand menuItem = new OleMenuCommand(ButtonBuildAndPushProjectExecuteHandler, ButtonBuildAndPushProjectChangeHandler, menuItem_BeforeQueryStatus, commandId);

                mcs.AddCommand(menuItem);
            }
        }

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand commandInfo = sender as OleMenuCommand;

            if (commandInfo != null)
            {

                DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

                var componentModel = (IComponentModel)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                IVsPackageInstaller installer = componentModel.GetService<IVsPackageInstaller>();

                dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

                Project currentProject = GetSelectedProject(dte);

                if (currentProject != null)
                {
                    if (installerServices.IsPackageInstalled(currentProject, packageId) == false)
                    {
                        commandInfo.Visible = false;
                    }
                    else
                    {
                        commandInfo.Visible = true;
                        commandInfo.Text = "Publish to Cloud Foundry";
                    }
                }
            }
        }

        private void ButtonBuildAndPushProjectChangeHandler(object sender, EventArgs e)
        {
        }

        private void ButtonBuildAndPushProjectExecuteHandler(object sender, EventArgs e)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
            Project currentProject = GetSelectedProject(dte);

            AppPackage projectPackage = new AppPackage();

            try
            {
                projectPackage.Initialize(currentProject);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayWarning(string.Format(CultureInfo.InvariantCulture, "Cannot load default profile. File is corrupt."));
                Logger.Error("Error loading default profile", ex);
            }

            var dialog = new EditDialog(projectPackage, currentProject);
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            dialog.ShowModal();


        }


        private Project GetSelectedProject(DTE dte)
        {
            foreach (EnvDTE.SelectedItem item in dte.SelectedItems)
            {
                Project current = item.Project as Project;
                if (current != null)
                {
                    return current;
                }

            }
            return null;
        }
        #endregion


    }
}
