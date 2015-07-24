namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;

    public class PushEnvironment
    {
        public const string DefaultProfileName = "push";
        public const string Extension = ".cf.pubxml";
        public const string DefaultWebsiteProjectName = "website.cfproj";
        public const string WebsitePublishingTargets = @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.WebSite.Publishing.targets";

        private string projectDirectory;
        private string profileFilePath;
        private string targetFilePath;
        private string projectName;
        private bool isProjectWebsite;

        public PushEnvironment(Project project)
        {
            var publishProfilePath = VsUtils.GetPublishProfilePath(project);
            this.targetFilePath = FileUtilities.GetRelativePath(publishProfilePath, VsUtils.GetTargetFile());
            this.isProjectWebsite = VsUtils.IsProjectWebsite(project);

            this.profileFilePath = Path.Combine(
                publishProfilePath,
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", DefaultProfileName, Extension));

            this.projectDirectory = VsUtils.GetProjectDirectory(project);

            if (project != null)
            {
                this.projectName = project.Name;
            }
        }

        public bool IsProjectWebsite
        {
            get { return this.isProjectWebsite; }
            set { this.isProjectWebsite = value; }
        }

        public string ProjectDirectory
        {
            get
            {
                return this.projectDirectory;
            }

            set
            {
                this.projectDirectory = value;
            }
        }

        public string ProfileFilePath
        {
            get
            {
                return this.profileFilePath;
            }

            set
            {
                this.profileFilePath = value;
            }
        }

        public string TargetFilePath
        {
            get
            {
                return this.targetFilePath;
            }

            set
            {
                this.targetFilePath = value;
            }
        }

        public string ProjectName
        {
            get
            {
                return this.projectName;
            }

            set
            {
                this.projectName = value;
            }
        }
    }
}
