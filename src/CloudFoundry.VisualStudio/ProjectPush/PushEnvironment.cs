using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    public class PushEnvironment
    {
        private string projectDirectory;
        private string profileFilePath;
        private string targetFilePath;
        private string projectName;

        public PushEnvironment()
        {
            this.projectDirectory = VsUtils.GetProjectDirectory();
            this.profileFilePath = Path.Combine(VsUtils.GetPublishProfilePath(), "push.cf.pubxml");
            this.targetFilePath = VsUtils.GetTargetFile();
            var project = VsUtils.GetSelectedProject();
            if (project != null)
            {
                this.projectName = project.Name;
            }


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
