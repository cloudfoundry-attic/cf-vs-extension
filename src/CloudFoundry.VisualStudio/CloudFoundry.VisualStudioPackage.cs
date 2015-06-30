namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using CloudFoundry.VisualStudio.Forms;
    using CloudFoundry.VisualStudio.Model;
    using CloudFoundry.VisualStudio.ProjectPush;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;
    using NuGet.VisualStudio;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    //// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(CloudFoundryExplorerToolWindow))]
    [Guid(GuidList.GuidCloudFoundryVisualStudioPkgString)]

    [ProvideEditorFactory(typeof(PublishXmlEditorFactory), 101)]
    [ProvideEditorExtension(typeof(PublishXmlEditorFactory), ".pubxml", 100)]

    // Our Editor supports Find and Replace therefore we need to declare support for LOGVIEWID_TextView.
    // This attribute declares that your EditorPane class implements IVsCodeWindow interface
    // used to navigate to find results from a "Find in Files" type of operation.
    [ProvideEditorLogicalView(typeof(PublishXmlEditorFactory), VSConstants.LOGVIEWID.TextView_string)]

    public sealed class CloudFoundry_VisualStudioPackage : Package
    {
        public const string PackageId = "cf-msbuild-tasks";
        public const string Extension = ".cf.pubxml";
        private static ErrorListProvider errorList;

        private DTE dte;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CloudFoundry_VisualStudioPackage()
        {
            // we need to force load an object from the Xceed assembly, otherwise the types don't get loaded on time
            var hackObject = new Xceed.Wpf.Toolkit.AutoSelectTextBox();
            hackObject = null;

            errorList = new ErrorListProvider(this);
            this.dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
        }

        public static ErrorListProvider GetErrorListPane()
        {
            return errorList;
        }

        public static string GetTargetFile()
        {
            string targetFile = string.Empty;
            try
            {
                var componentModel = (IComponentModel)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                var packageDir = installerServices.GetInstalledPackages().Where(o => o.Id == CloudFoundry_VisualStudioPackage.PackageId).FirstOrDefault().InstallPath;

                targetFile = System.IO.Path.Combine(packageDir, "cf-msbuild-tasks.targets");
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving target file", ex);
            }
            return targetFile;
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
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            this.RegisterEditorFactory(new PublishXmlEditorFactory());

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.GuidCloudFoundryVisualStudioCmdSet, (int)PkgCmdIDList.CmdidCloudFoundryExplorer);
                MenuCommand menuToolWin = new MenuCommand(this.ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);

                // Create the command for button ButtonBuildAndPushProject
                CommandID commandId = new CommandID(GuidList.GuidCloudFoundryVisualStudioCmdSet, (int)PkgCmdIDList.CmdidButtonPublishProject);
                OleMenuCommand menuItem = new OleMenuCommand(this.ButtonBuildAndPushProjectExecuteHandler, this.ButtonBuildAndPushProjectChangeHandler, this.MenuItem_BeforeQueryStatus, commandId);

                mcs.AddCommand(menuItem);

                CommandID websiteId = new CommandID(GuidList.GuidCloudFoundryVisualStudioCmdSet, (int)PkgCmdIDList.CmdidButtonPublishWebSite);
                OleMenuCommand menuWebSite = new OleMenuCommand(this.ButtonBuildAndPushProjectExecuteHandler, this.ButtonBuildAndPushProjectChangeHandler, this.MenuItem_BeforeQueryStatus, websiteId);

                mcs.AddCommand(menuWebSite);
            }
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand commandInfo = sender as OleMenuCommand;

            if (commandInfo != null)
            {
                DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

                var componentModel = (IComponentModel)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

                if (installerServices.GetInstalledPackages().Where(o => o.Id == PackageId).FirstOrDefault() == null)
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

        private void ButtonBuildAndPushProjectChangeHandler(object sender, EventArgs e)
        {
        }

        private void ButtonBuildAndPushProjectExecuteHandler(object sender, EventArgs e)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
            Project currentProject = this.GetSelectedProject(dte);

            try
            {
                var package = PublishProfile.Load(currentProject, Path.Combine(GetPublishProfilePath(currentProject),"push.cf.pubxml") , GetTargetFile());

                var dialog = new PushDialog(package);
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayWarning(string.Format(CultureInfo.InvariantCulture, "Cannot load default profile. File is corrupt."));
                Logger.Error("Error loading default profile", ex);
            }
        }

        public static string GetPublishProfilePath(Project project)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

            IVsSolution solution = (IVsSolution)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(IVsSolution));

            var solutionDirectory = Path.GetDirectoryName(dte.Solution.FullName);
            var projectFileFullPath = Path.Combine(solutionDirectory, project.UniqueName);

            string projectFolder = Path.GetDirectoryName(projectFileFullPath);

            IVsHierarchy hierarchy;

            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            IVsAggregatableProject aggregatable = (IVsAggregatableProject)hierarchy;

            string projectTypes = string.Empty;
            aggregatable.GetAggregateProjectTypeGuids(out projectTypes);

            if (projectTypes.ToUpperInvariant().Contains("{E24C65DC-7377-472B-9ABA-BC803B73C61A}"))
            {
                return System.IO.Path.Combine(projectFolder, "AppData", "PublishProfiles");
            }

            if (projectTypes.ToUpperInvariant().Contains("{349C5851-65DF-11DA-9384-00065B846F21}"))
            {
                return System.IO.Path.Combine(projectFolder, "Properties", "PublishProfiles");
            }

            return System.IO.Path.Combine(projectFolder, "PublishProfiles");
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
    }
}
