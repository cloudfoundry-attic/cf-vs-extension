using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CloudFoundry.VisualStudio
{
    internal class VsUtils
    {
        public static string GetPublishProfilePath(Project project)
        {
            IVsSolution solution = (IVsSolution)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(IVsSolution));
            if (solution == null)
            {
                return string.Empty;
            }

            string projectFolder = GetProjectDirectory(project);

            IVsHierarchy hierarchy;

            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            IVsAggregatableProject aggregatable = (IVsAggregatableProject)hierarchy;

            string projectTypes = string.Empty;
            aggregatable.GetAggregateProjectTypeGuids(out projectTypes);

            if (projectTypes.ToUpperInvariant().Contains("{E24C65DC-7377-472B-9ABA-BC803B73C61A}"))
            {
                return System.IO.Path.Combine(projectFolder, "App_Data", "PublishProfiles");
            }

            if (projectTypes.ToUpperInvariant().Contains("{349C5851-65DF-11DA-9384-00065B846F21}"))
            {
                return System.IO.Path.Combine(projectFolder, "Properties", "PublishProfiles");
            }

            return System.IO.Path.Combine(projectFolder, "PublishProfiles");
        }

        public static string GetProjectDirectory(Project project)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
            if (dte == null)
            {
                return string.Empty;
            }
            if (project == null)
            {
                return string.Empty;
            }

            var solutionDirectory = Path.GetDirectoryName(dte.Solution.FullName);

            string projectFolder = solutionDirectory;

            foreach (Property prop in project.Properties)
            {
                if (prop.Name == "FullPath" || prop.Name == "ProjectDirectory")
                {
                    projectFolder = prop.Value.ToString();
                    break;
                }
            }
            return projectFolder;
        }

        public static Project GetSelectedProject()
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));
            if (dte == null)
            {
                return null;
            }
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

        public static string GetTargetFile()
        {
            string targetFile = string.Empty;
            try
            {
                var componentModel = (IComponentModel)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));
                if (componentModel == null)
                {
                    return string.Empty;
                }

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
    }


}
