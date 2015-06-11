namespace CloudFoundry.VisualStudio.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Build.Evaluation;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;

    public class MSBuildProcess
    {
        public Dictionary<string, string> MSBuildProperties { get; set; }

        public void Publish(string projectFullPath)
        {
            this.Publish(projectFullPath, Microsoft.Build.Framework.LoggerVerbosity.Normal);
        }

        public void Publish(string projectFullPath, Microsoft.Build.Framework.LoggerVerbosity verbosity)
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

                var engine = ProjectCollection.GlobalProjectCollection;

                ErrorListProvider errorList = CloudFoundry_VisualStudioPackage.GetErrorListPane();

                if (errorList == null)
                {
                    throw new InvalidOperationException("Could not retrieve error list provider");
                }

                errorList.Tasks.Clear();

                MSBuildLogger customLogger = new MSBuildLogger(pane, errorList);

                customLogger.Verbosity = verbosity;

                engine.RegisterLogger(customLogger);

                foreach (KeyValuePair<string, string> parameter in MSBuildProperties)
                {
                    engine.SetGlobalProperty(parameter.Key, parameter.Value);
                }

                foreach (var projectItem in engine.LoadedProjects)
                {
                    if (projectItem.FullPath.Equals(projectFullPath))
                    {
                        projectItem.Build(new string[] { "Build", "WebCloudFoundryPublish" });
                    }
                }

                if (errorList.Tasks.Count > 0)
                {
                    errorList.Show();
                }

                engine.UnregisterAllLoggers();
            });
        }
    }
}
