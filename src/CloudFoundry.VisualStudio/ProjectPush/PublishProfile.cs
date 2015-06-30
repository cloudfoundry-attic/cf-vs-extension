namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using CloudFoundry.VisualStudio.Forms;
    using EnvDTE;
    using CloudFoundry.Manifests;
    using CloudFoundry.Manifests.Models;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using System.Collections.ObjectModel;
    using CloudFoundry.VisualStudio.TargetStore;
    using CloudFoundry.UAA;
    using System.Threading;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.ComponentModelHost;
    using NuGet.VisualStudio;


    [ComVisible(true)]
    [Serializable, XmlRoot("PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class PublishProfile : INotifyPropertyChanged
    {
        private Project project;
        private string path;
        private XmlSerializerNamespaces namespaces = null;
      
        private string user;
        private string password;
        private bool savedPassword;
        private string refreshToken;
        private XmlUri serverUri;
        private bool skipSSLValidation;
        private string organization;
        private string space;
        private string deployTargetFile;
        private string webPublishMethod;
        private string manifest;

        [XmlIgnore]
        public Project Project
        {
            get { return project; }
            set { project = value; }
        }

        [XmlIgnore]
        public string Path
        {
            get
            {
                return this.path;
            }
        }

        [XmlElement(ElementName = "CFUser")]
        public string User
        {
            get
            {
                return this.user;
            }
            set
            {
                this.user = value;
                RaisePropertyChangedEvent("User");
            }
        }

        [XmlElement(ElementName = "CFPassword")]
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
                RaisePropertyChangedEvent("Password");
            }
        }

        [XmlElement(ElementName = "CFSavedPassword")]
        public bool SavedPassword
        {
            get
            {
                return this.savedPassword;
            }
            set
            {
                this.savedPassword = value;
                RaisePropertyChangedEvent("SavedPassword");
            }
        }

        [XmlElement(ElementName = "CFRefreshToken")]
        public string RefreshToken
        {
            get
            {
                return this.refreshToken;
            }
            set
            {
                this.refreshToken = value;
                RaisePropertyChangedEvent("RefreshToken");
            }
        }

        [XmlElement(ElementName = "CFServerUri")]
        public XmlUri ServerUri
        {
            get
            {
                return this.serverUri;
            }
            set
            {
                this.serverUri = value;
                RaisePropertyChangedEvent("ServerUri");
            }
        }

        [XmlElement(ElementName = "CFSkipSSLValidation")]
        public bool SkipSSLValidation
        {
            get
            {
                return this.skipSSLValidation;
            }
            set
            {
                this.skipSSLValidation = value;
                RaisePropertyChangedEvent("SkipSSLValidation");
            }
        }

        [XmlElement(ElementName = "CFOrganization")]
        public string Organization
        {
            get
            {
                return this.organization;
            }
            set
            {
                this.organization = value;
                RaisePropertyChangedEvent("Organization");
            }
        }

        [XmlElement(ElementName = "CFSpace")]
        public string Space
        {
            get
            {
                return this.space;
            }
            set
            {
                this.space = value;
                RaisePropertyChangedEvent("Space");
            }
        }

        [XmlElement(ElementName = "DeployTargetFile")]
        public string DeployTargetFile
        {
            get
            {
                return this.deployTargetFile;
            }
            set
            {
                this.deployTargetFile = value;
                RaisePropertyChangedEvent("DeployTargetFile");
            }
        }

        [XmlElement(ElementName = "WebPublishMethod")]
        public string WebPublishMethod
        {
            get
            {
                return this.webPublishMethod;
            }
            set
            {
                this.webPublishMethod = value;
                RaisePropertyChangedEvent("WebPublishMethod");
            }
        }

        [XmlElement(ElementName = "CFManifest")]
        public string Manifest
        {
            get
            {
                return this.manifest;
            }
            set
            {
                this.manifest = value;
                RaisePropertyChangedEvent("Manifest");
            }
        }

        [XmlIgnore]
        public Application Application
        {
            get;
            private set;
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        [XmlIgnore]
        public string TargetFile 
        { 
            get; 
            set; 
        }

        private PublishProfile()
        {
            this.namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] {
                new XmlQualifiedName(string.Empty, "http://schemas.microsoft.com/developer/msbuild/2003")
            });
        }

        private void LoadManifest()
        {
            string projectDir = System.IO.Path.GetDirectoryName(project.FullName);
            string absoluteManifestPath = System.IO.Path.Combine(projectDir, this.Manifest);


            if (File.Exists(absoluteManifestPath))
            {
                var manifest = ManifestDiskRepository.ReadManifest(absoluteManifestPath);

                if (manifest.Applications().Count() > 1)
                {
                    throw new FileFormatException("Invalid CloudFoundry manifest file: more than one application is configured.");
                }
                else if (manifest.Applications().Count() < 1)
                {
                    throw new FileFormatException("Invalid CloudFoundry manifest file: there is no application configured.");
                }

                this.Application = manifest.Applications().First();
            }
            else
            {
                this.Application = new Application()
                {
                    BuildpackUrl = string.Empty,
                    Command = null,
                    DiskQuota = null,
                    HealthCheckTimeout = null,
                    InstanceCount = 1,
                    Memory = 256,
                    Name = project.Name,
                    NoHostName = false,
                    NoRoute = false,
                    Path = null,
                    StackName = string.Empty,
                    UseRandomHostName = false
                };

                this.Application.Hosts.Add(project.Name.ToLowerInvariant());
            }
        }

        private void SaveManifest()
        {
            string projectDir = System.IO.Path.GetDirectoryName(project.FullName);
            Directory.CreateDirectory(projectDir);

            string absoluteManifestPath = System.IO.Path.Combine(projectDir, this.Manifest);

            this.Application.Path = projectDir;
            this.Manifest = absoluteManifestPath;

            CloudFoundry.Manifests.Manifest.Save(new Application[] { this.Application }, absoluteManifestPath);
        }

        /// <summary>
        /// Loads the specified publish profile for the specified project.
        /// </summary>
        /// <param name="project">The Visual Studio EnvDTE project. Cannot be null.</param>
        /// <param name="path">Absolute path to the publish profile to load. If the file does not exist, defaults will be loaded for the object.</param>
        /// <returns>A new PublishProfile.</returns>
        /// <exception cref="System.ArgumentNullException">project</exception>
        public static PublishProfile Load(Project project, string path, string targetFile)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            PublishProfile publishProfile = null;

            if (File.Exists(path))
            {
                // If the file exists, we load it
                XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile));

                using (XmlReader xmlReader = XmlReader.Create(path))
                {
                    xmlReader.ReadToDescendant("PropertyGroup");
                    publishProfile = (PublishProfile)serializer.Deserialize(xmlReader.ReadSubtree());
                }
            }
            else
            {
                // If the file does not exist, we set defaults
                publishProfile = new PublishProfile()
                {
                    Manifest = "manifest.yml",
                    Organization = string.Empty,
                    Password = null,
                    RefreshToken = null,
                    SavedPassword = true,
                    ServerUri = null,
                    SkipSSLValidation = false,
                    Space = string.Empty,
                    User = string.Empty,
                    DeployTargetFile = null,
                    WebPublishMethod = "CloudFoundry"
                };
            }

            publishProfile.TargetFile = targetFile;
            publishProfile.path = path;
            publishProfile.project = project;
            publishProfile.LoadManifest();

            return publishProfile;
        }

        /// <summary>
        /// Writes this instance as XML in the publish profile file.
        /// It also writes the Application manifest to its corresponding YAML file.
        /// </summary>
        public void Save()
        {
            SaveManifest();

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile),
                    new XmlRootAttribute("PropertyGroup") { Namespace = "http://schemas.microsoft.com/developer/msbuild/2003" });

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (StringWriter textWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    xmlWriter.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                    xmlWriter.WriteAttributeString("ToolsVersion", "4.0");

                    xmlWriter.WriteStartElement("Import");
                    xmlWriter.WriteAttributeString("Project", this.TargetFile);
                    xmlWriter.WriteEndElement();

                    serializer.Serialize(xmlWriter, this, this.Namespaces);

                    xmlWriter.WriteEndElement();
                }

                File.WriteAllText(this.path, textWriter.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
