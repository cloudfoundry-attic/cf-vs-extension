namespace CloudFoundry.VisualStudio.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.Build.Evaluation;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Execution;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio;
    using EnvDTE80;
    using System.IO;

    public class MSBuildProcess
    {
        public Dictionary<string, string> MSBuildProperties { get; set; }

        public void Publish(EnvDTE.Project project)
        {
            this.Publish(project, Microsoft.Build.Framework.LoggerVerbosity.Normal);
        }

        public void Publish(EnvDTE.Project project, Microsoft.Build.Framework.LoggerVerbosity verbosity)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
                var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                var output = (OutputWindow)window.Object;

                OutputWindowPane pane = null;

                pane = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "Publish");

                if (pane == null)
                {
                    pane = output.OutputWindowPanes.Add("Publish");
                }

                pane.Activate();

                ErrorListProvider errorList = CloudFoundry_VisualStudioPackage.GetErrorListPane();

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
                            string projectDir = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(MSBuildProperties["PublishProfile"])).Parent.Parent.FullName;
                            proj = Path.Combine(projectDir, CloudFoundry.VisualStudio.ProjectPush.PushEnvironment.DefaultWebsiteProjName);
                        }

                        websiteProject = projectCollection.LoadProject(proj);

                        foreach (KeyValuePair<string, string> parameter in MSBuildProperties)
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
