namespace CloudFoundry.VisualStudio.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    
    public static class MSBuildProcess
    {
        public static void Publish(EnvDTE.Project project, Dictionary<string, string> buildProperties)
        {
            Publish(project, buildProperties, Microsoft.Build.Framework.LoggerVerbosity.Normal);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Tight coupling needed")]
        public static void Publish(EnvDTE.Project project, Dictionary<string, string> buildProperties, Microsoft.Build.Framework.LoggerVerbosity verbosity)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (buildProperties == null)
            {
                throw new ArgumentNullException("buildProperties");
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                DTE dte = (DTE)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(DTE));
                var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                var output = (OutputWindow)window.Object;

                OutputWindowPane pane = null;

                pane = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "Publish");

                if (pane == null)
                {
                    pane = output.OutputWindowPanes.Add("Publish");
                }

                var showOutputWindowPropertyValue = VsUtils.GetVisualStudioSetting("Environment", "ProjectsAndSolution", "ShowOutputWindowBeforeBuild");

                if (showOutputWindowPropertyValue != null)
                {
                    if ((bool)showOutputWindowPropertyValue == true)
                    {
                        window.Visible = true;
                    }
                }

                pane.Activate();

                ErrorListProvider errorList = CloudFoundryVisualStudioPackage.GetErrorListPane;

                if (errorList == null)
                {
                    throw new InvalidOperationException("Could not retrieve error list provider");
                }

                errorList.Tasks.Clear();

                MSBuildLogger customLogger = new MSBuildLogger(pane, errorList);

                customLogger.Verbosity = verbosity;
                pane.Clear();

                pane.OutputString("Starting push...");

                Microsoft.Build.Evaluation.Project websiteProject = null;

                using (var buildManager = new BuildManager())
                {
                    using (var projectCollection = new ProjectCollection())
                    {
                        string proj = project.FullName;

                        if (project.Object is VsWebSite.VSWebSite)
                        {
                            string projectDir = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(buildProperties["PublishProfile"])).Parent.Parent.FullName;
                            proj = Path.Combine(projectDir, CloudFoundry.VisualStudio.ProjectPush.PushEnvironment.DefaultWebsiteProjectName);
                        }

                        websiteProject = projectCollection.LoadProject(proj);

                        foreach (KeyValuePair<string, string> parameter in buildProperties)
                        {
                            websiteProject.SetProperty(parameter.Key, parameter.Value);
                        }

                        BuildParameters buildParameters = new BuildParameters(projectCollection);
                        buildParameters.Loggers = new List<ILogger>() { customLogger };
                        BuildRequestData buildRequestData = new BuildRequestData(websiteProject.CreateProjectInstance(), new string[] { });

                        buildManager.Build(buildParameters, buildRequestData);
                        if (errorList.Tasks.Count > 0)
                        {
                            errorList.BringToFront();
                        }

                        pane.OutputString("Push operation finished!");
                        projectCollection.UnregisterAllLoggers();
                    }
                }
            });
        }
    }
}