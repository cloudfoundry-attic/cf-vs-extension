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

namespace HP.CloudFoundry.UI.VisualStudio
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
    [Guid(GuidList.guidHP_CloudFoundry_UI_VisualStudioPkgString)]
    public sealed class HP_CloudFoundry_UI_VisualStudioPackage : Package
    {
        private const string packageId = "cf-msbuild-tasks";
        private const string packageSource = "http://nuget.15.126.229.131.xip.io/nuget/";
    
        List<int> dynamicExtenderProviderCookies = new List<int>();
        ObjectExtenders extensionManager;
    
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public HP_CloudFoundry_UI_VisualStudioPackage()
        {
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

        private void RegisterExtensions()
        {
            extensionManager = (ObjectExtenders)GetService(typeof(ObjectExtenders));
            if (extensionManager == null)
            {
                throw new InvalidOperationException("GetService failed to get the extender object");
            }

            CloudProjectExtenderProvider dynamicExtenderProvider = new CloudProjectExtenderProvider();

            foreach (string objectToExtend in CloudProjectExtenderProvider.ProjectTypesToExtend)
            {
                dynamicExtenderProviderCookies.Add(extensionManager.RegisterExtenderProvider(
                    objectToExtend, CloudProjectExtenderProvider.DynamicExtenderName, dynamicExtenderProvider));
            }
        }

        private void UnregisterExtensions()
        {
            if (extensionManager != null)
            {
                foreach (int dynamicExtenderProviderCookie in dynamicExtenderProviderCookies)
                {
                    if (dynamicExtenderProviderCookie != 0)
                    {
                        extensionManager.UnregisterExtenderProvider(dynamicExtenderProviderCookie);
                    }
                }
            }
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
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            RegisterExtensions();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidHP_CloudFoundry_UI_VisualStudioCmdSet, (int)PkgCmdIDList.cmdidCloudFoundryExplorer);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );

                // Create the command for button ButtonBuildAndPushProject
                CommandID commandId = new CommandID(GuidList.guidHP_CloudFoundry_UI_VisualStudioCmdSet, (int)PkgCmdIDList.cmdidButtonPublishProject);
                OleMenuCommand menuItem = new OleMenuCommand(ButtonBuildAndPushProjectExecuteHandler, ButtonBuildAndPushProjectChangeHandler, menuItem_BeforeQueryStatus, commandId);

                mcs.AddCommand(menuItem);
            }
        }

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand commandInfo = sender as OleMenuCommand;

            if (commandInfo != null)
            {

                DTE dte = (DTE)HP_CloudFoundry_UI_VisualStudioPackage.GetGlobalService(typeof(DTE));

                var componentModel = (IComponentModel)HP_CloudFoundry_UI_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                IVsPackageInstaller installer = componentModel.GetService<IVsPackageInstaller>();

                dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

                Project currentProject = GetSelectedProject(dte);

                if (currentProject != null)
                {
                    if (installerServices.IsPackageInstalled(currentProject, packageId) == false)
                    {
                        commandInfo.Text = "Enable CF Publish";
                    }
                    else
                    {
                        commandInfo.Text = "Publish to CF";
                    }
                }
            }
        }

        private void ButtonBuildAndPushProjectChangeHandler(object sender, EventArgs e)
        {
        }

        private async void ButtonBuildAndPushProjectExecuteHandler(object sender, EventArgs e)
        {
            OleMenuCommand commandInfo = sender as OleMenuCommand;
            if (commandInfo != null)
            {
                DTE dte = (DTE)HP_CloudFoundry_UI_VisualStudioPackage.GetGlobalService(typeof(DTE));

                var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                var output = (OutputWindow)window.Object;
                OutputWindowPane pane = output.OutputWindowPanes.Add("Publish");

                if (commandInfo.Text.ToLower().Contains("enable"))
                {
                    var componentModel = (IComponentModel)HP_CloudFoundry_UI_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                    IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                    IVsPackageInstaller installer = componentModel.GetService<IVsPackageInstaller>();

                    dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

                    Project currentProject = GetSelectedProject(dte);

                    if (currentProject != null)
                    {
                        pane.OutputString(string.Format(CultureInfo.InvariantCulture, "Please wait for {0} to be installed...", packageId));
                        await System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            installer.InstallPackage(packageSource, currentProject, packageId, (Version)null, false);

                            try{
                            //Force reload to update extension properties
                            dte.ExecuteCommand("Project.UnloadProject");
                            dte.ExecuteCommand("Project.ReloadProject");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Exeception occured on project force reload {0}", ex.Message));
                            }
                        });
                    }
                }
                else
                {
                    dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

                    Project currentProject = GetSelectedProject(dte);

                    if (currentProject != null)
                    {
                        string msBuildPath = Microsoft.Build.Utilities.ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", "12.0");
                        //string msBuildPath = System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "msbuild.exe");
                        string projectPath = currentProject.FullName;

                        await System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            var startInfo = new ProcessStartInfo(msBuildPath)
                            {
                                Arguments = string.Format(CultureInfo.InvariantCulture, @"/p:DeployOnBuild=true;PublishProfile=cf-push ""{0}""", projectPath),
                                WorkingDirectory = System.IO.Path.GetDirectoryName(projectPath),
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            pane.OutputString("> msbuild " + startInfo.Arguments);
                            var process = System.Diagnostics.Process.Start(startInfo);

                            string outline = string.Empty;
                            while ((outline = process.StandardOutput.ReadLine()) != null)
                            {
                                pane.OutputString(outline + Environment.NewLine);
                            }
                            process.WaitForExit();
                        });
                    }
                }
            }
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

        public void Dispose()
        {
            UnregisterExtensions();
        }
    }
}
