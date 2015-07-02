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
        private string name;
        private PushEnvironment environment;


        [XmlIgnore]
        public string Path
        {
            get
            {
                return this.path;
            }
        }

        [XmlIgnore]
        public string Name
        {
            get
            {
                this.name = this.GetProfileName();

                return this.name;
            }
            set
            {
                var profileDir = System.IO.Directory.GetParent(this.environment.ProfileFilePath).FullName;

                var fileName = string.Format(CultureInfo.InvariantCulture, "{0}.cf.pubxml", value);
                this.path = System.IO.Path.Combine(profileDir, fileName);
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
            string projectDir = this.environment.ProjectDirectory;
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
                    Name = this.environment.ProjectName,
                    NoHostName = false,
                    NoRoute = false,
                    Path = null,
                    StackName = string.Empty,
                    UseRandomHostName = false
                };

                this.Application.Hosts.Add(this.environment.ProjectName.ToLowerInvariant());
            }
        }

        private void SaveManifest()
        {
            string projectDir = this.environment.ProjectDirectory;
            Directory.CreateDirectory(projectDir);

            this.Application.Path = projectDir;

            string absoluteManifestPath = System.IO.Path.Combine(projectDir, this.Manifest);

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
        public static PublishProfile Load(PushEnvironment pushEnvironment)
        {

            PublishProfile publishProfile = null;

            if (File.Exists(pushEnvironment.ProfileFilePath))
            {
                // If the file exists, we load it
                XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile));

                using (XmlReader xmlReader = XmlReader.Create(pushEnvironment.ProfileFilePath))
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

                publishProfile.path = pushEnvironment.ProfileFilePath;
                publishProfile.Manifest = string.Format(CultureInfo.InvariantCulture, "{0}.yml", publishProfile.GetProfileName());
            }

            publishProfile.TargetFile = pushEnvironment.TargetFilePath;
            publishProfile.path = pushEnvironment.ProfileFilePath;
            publishProfile.environment = pushEnvironment;
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

        private string GetProfileName()
        {
            if (string.IsNullOrWhiteSpace(this.path))
            {
                return string.Empty;
            }

            string fullFileName = System.IO.Path.GetFileName(this.path);
            if (!fullFileName.EndsWith(CloudFoundry_VisualStudioPackage.Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            return fullFileName.Substring(0, fullFileName.Length - CloudFoundry_VisualStudioPackage.Extension.Length);
        }
    }
}
