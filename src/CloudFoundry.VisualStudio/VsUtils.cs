namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;

    internal class VsUtils
    {
        public static bool IsProjectWebsite(Project project)
        {
            if (project != null)
            {
                return project.Object is VsWebSite.VSWebSite;
            }
            else
            {
                return false;
            }
        }

        public static object GetVisualStudioSetting(string category, string page, string propertyName)
        {
            DTE dte = (DTE)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(DTE));

            if (dte == null)
            {
                return null;
            }

            EnvDTE.Properties propertyList = dte.get_Properties(category, page);
            foreach (EnvDTE.Property prop in propertyList)
            {
                if (prop.Name == propertyName)
                {
                    return prop.Value;
                }
            }

            return null;
        }

        public static string GetPublishProfilePath(Project project)
        {
            IVsSolution solution = (IVsSolution)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(IVsSolution));
            if (solution == null)
            {
                return string.Empty;
            }

            string projectFolder = GetProjectDirectory(project);

            IVsHierarchy hierarchy;

            int getProjectResult = solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            if (getProjectResult != VSConstants.S_OK)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error retrieving project of unique name, error code: {0}", getProjectResult));
            }

            IVsAggregatableProject aggregatable = (IVsAggregatableProject)hierarchy;

            string projectTypes = string.Empty;
            int projectTypeResult = aggregatable.GetAggregateProjectTypeGuids(out projectTypes);

            if (projectTypeResult != VSConstants.S_OK)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error retrieving aggregate project types, error code: {0}", projectTypeResult));
            }

            string result = System.IO.Path.Combine(projectFolder, "PublishProfiles");

            if (projectTypes.ToUpperInvariant().Contains("{E24C65DC-7377-472B-9ABA-BC803B73C61A}"))
            {
                result = System.IO.Path.Combine(projectFolder, "App_Data", "PublishProfiles");
            }

            if (projectTypes.ToUpperInvariant().Contains("{349C5851-65DF-11DA-9384-00065B846F21}"))
            {
                result = System.IO.Path.Combine(projectFolder, "Properties", "PublishProfiles");
            }

            return FileUtilities.PathAddBackslash(result);
        }

        public static string GetProjectDirectory(Project project)
        {
            DTE dte = (DTE)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(DTE));
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
            DTE dte = (DTE)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(DTE));
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

            return GetProjectFromSelection();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Prevent crash if nuget service isn't working")]
        public static string GetTargetFile()
        {
            string targetFile = string.Empty;
            try
            {
                var componentModel = (IComponentModel)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(SComponentModel));
                if (componentModel == null)
                {
                    return string.Empty;
                }

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                var packageDir = installerServices.GetInstalledPackages().Where(o => o.Id == CloudFoundryVisualStudioPackage.PackageId).FirstOrDefault().InstallPath;

                targetFile = System.IO.Path.Combine(packageDir, "cf-msbuild-tasks.targets");
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving nuget package service", ex);
            }

            return targetFile;
        }

        private static Project GetProjectFromSelection()
        {
            IntPtr hierarchyPointer, selectionContainerPointer;
            uint projectItemId;
            IVsMultiItemSelect multiItemSelect;

            IVsMonitorSelection monitorSelection = (IVsMonitorSelection)CloudFoundryVisualStudioPackage.GetGlobalService(typeof(SVsShellMonitorSelection));

            int currentSelectionResult = monitorSelection.GetCurrentSelection(out hierarchyPointer, out projectItemId, out multiItemSelect, out selectionContainerPointer);

            if (currentSelectionResult != VSConstants.S_OK)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error retrieving current selection from the selection monitor, error code {0}", currentSelectionResult));
            }

            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPointer, typeof(IVsHierarchy)) as IVsHierarchy;

            object objProj;

            int getPropertyResult = selectedHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);

            if (getPropertyResult != VSConstants.S_OK)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error getting property from the selected hierarchy, error code {0}", getPropertyResult));
            }

            var project = objProj as EnvDTE.Project;

            return project;
        }
    }
}
