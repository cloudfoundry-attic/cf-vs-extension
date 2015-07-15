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

    public class MSBuildProcess
    {
        public Dictionary<string, string> MSBuildProperties { get; set; }

        public void Publish()
        {
            this.Publish(Microsoft.Build.Framework.LoggerVerbosity.Normal);
        }

        public void Publish(Microsoft.Build.Framework.LoggerVerbosity verbosity)
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

                using (var projectCollection = new ProjectCollection())
                {
                    websiteProject = projectCollection.LoadProject(MSBuildProperties["SelectedPublishProfile"]);

                    foreach (KeyValuePair<string, string> parameter in MSBuildProperties)
                    {
                        websiteProject.SetProperty(parameter.Key, parameter.Value);
                    }

                    websiteProject.Build("WebCloudFoundryPublish", new List<ILogger>() { customLogger });

                    if (errorList.Tasks.Count > 0)
                    {
                        errorList.Show();
                    }
                    pane.OutputString("Push operation finished!");
                    projectCollection.UnregisterAllLoggers();
                }
            });
        }
    }
}
