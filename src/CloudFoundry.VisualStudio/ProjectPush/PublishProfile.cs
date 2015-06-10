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
                RaisePropertyChangedEvent("User");
                this.user = value;
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
                RaisePropertyChangedEvent("Password");
                this.password = value;
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
                RaisePropertyChangedEvent("SavedPassword");
                this.savedPassword = value;
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
                RaisePropertyChangedEvent("RefreshToken");
                this.refreshToken = value;
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
                RaisePropertyChangedEvent("ServerUri");
                this.serverUri = value;
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
                RaisePropertyChangedEvent("SkipSSLValidation");
                this.skipSSLValidation = value;
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
                RaisePropertyChangedEvent("Organization");
                this.organization = value;
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
                RaisePropertyChangedEvent("Space");
                this.space = value;
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
                RaisePropertyChangedEvent("DeployTargetFile");
                this.deployTargetFile = value;
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
                RaisePropertyChangedEvent("WebPublishMethod");
                this.webPublishMethod = value;
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
                RaisePropertyChangedEvent("Manifest");
                this.manifest = value;
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
                CloudFoundry.Manifests.ManifestDiskRepository manifestRepo = new ManifestDiskRepository();


                var manifest = manifestRepo.ReadManifest(absoluteManifestPath);

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
                    Domains = new string[0],
                    EnvironmentVars = new Dictionary<string, string>(),
                    HealthCheckTimeout = null,
                    Hosts = new string[] { project.Name.ToLowerInvariant() },
                    InstanceCount = 1,
                    Memory = 256,
                    Name = project.Name,
                    NoHostname = false,
                    NoRoute = false,
                    Path = null,
                    ServicesToBind = new string[0],
                    StackName = string.Empty,
                    UseRandomHostname = false
                };
            }
        }

        private void SaveManifest()
        {
            string projectDir = System.IO.Path.GetDirectoryName(project.FullName);
            Directory.CreateDirectory(projectDir);

            string absoluteManifestPath = System.IO.Path.Combine(projectDir, this.Manifest);

            CloudFoundry.Manifests.Manifest.Save(new Application[] { this.Application }, absoluteManifestPath);
        }

        /// <summary>
        /// Loads the specified publish profile for the specified project.
        /// </summary>
        /// <param name="project">The Visual Studio EnvDTE project. Cannot be null.</param>
        /// <param name="path">Absolute path to the publish profile to load. If the file does not exist, defaults will be loaded for the object.</param>
        /// <returns>A new PublishProfile.</returns>
        /// <exception cref="System.ArgumentNullException">project</exception>
        public static PublishProfile Load(Project project, string path)
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

    internal enum PublishProfileRefreshTarget
    {
        Client,
        Organizations,
        Spaces,
        ServiceInstances,
        Stacks,
        Buildpacks,
        SharedDomains,
        PrivateDomains
    }

    internal class PublishProfileEditorResources
    {
        private ObservableCollection<ListAllOrganizationsResponse> orgs = new ObservableCollection<ListAllOrganizationsResponse>();
        private ObservableCollection<ListAllSpacesForOrganizationResponse> spaces = new ObservableCollection<ListAllSpacesForOrganizationResponse>();
        private ObservableCollection<ListAllStacksResponse> stacks = new ObservableCollection<ListAllStacksResponse>();
        private ObservableCollection<ListAllBuildpacksResponse> buildpacks = new ObservableCollection<ListAllBuildpacksResponse>();
        private ObservableCollection<ListAllSharedDomainsResponse> sharedDomains = new ObservableCollection<ListAllSharedDomainsResponse>();
        private ObservableCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = new ObservableCollection<ListAllPrivateDomainsForOrganizationResponse>();
        private ObservableCollection<ListAllServiceInstancesForSpaceResponse> serviceInstances = new ObservableCollection<ListAllServiceInstancesForSpaceResponse>();

        public ObservableCollection<ListAllOrganizationsResponse> Orgs
        {
            get { return orgs; }
            set { orgs = value; }
        }

        public ObservableCollection<ListAllSpacesForOrganizationResponse> Spaces
        {
            get { return spaces; }
            set { spaces = value; }
        }

        public ObservableCollection<ListAllStacksResponse> Stacks
        {
            get { return stacks; }
            set { stacks = value; }
        }

        public ObservableCollection<ListAllBuildpacksResponse> Buildpacks
        {
            get { return buildpacks; }
            set { buildpacks = value; }
        }

        public ObservableCollection<ListAllSharedDomainsResponse> SharedDomains
        {
            get { return sharedDomains; }
            set { sharedDomains = value; }
        }

        public ObservableCollection<ListAllPrivateDomainsForOrganizationResponse> PrivateDomains
        {
            get { return privateDomains; }
            set { privateDomains = value; }
        }

        public ObservableCollection<ListAllServiceInstancesForSpaceResponse> ServiceInstances
        {
            get { return serviceInstances; }
            set { serviceInstances = value; }
        }

        public PublishProfile PublishProfile
        {
            get { return publishProfile; }
            set { publishProfile = value; }
        }

        public PublishProfileRefreshTarget LastRefreshTarget
        {
            get;
            set;
        }

        private PublishProfile publishProfile;


        private bool hasErrors = false;
        private string errorMessage = string.Empty;
        private bool refreshing = false;
        private CancellationToken cancellationToken;
        private CloudFoundryClient client;

        public bool HasErrors
        {
            get { return hasErrors; }
            set { hasErrors = value; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        public bool Refreshing
        {
            get { return refreshing; }
            set { refreshing = value; }
        }

        public PublishProfileEditorResources(PublishProfile publishProfile, CancellationToken cancellationToken)
        {
            this.publishProfile = publishProfile;
            this.publishProfile.PropertyChanged += publishProfile_PropertyChanged;
            this.cancellationToken = cancellationToken;
        }

        private void publishProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Organization":
                    {
                        this.RefreshSpaces().Forget();
                        this.RefreshPrivateDomains().Forget();
                    }
                    break;
                case "Space":
                    {
                        this.RefreshServiceInstances().Forget();
                    }
                    break;
            }
        }

        private void EnterRefresh()
        {
            this.Refreshing = true;
            this.HasErrors = false;
            this.ErrorMessage = string.Empty;
        }

        private void ExitRefresh()
        {
            this.ExitRefresh(null);
        }

        private void ExitRefresh(Exception error)
        {
            this.Refreshing = false;
            this.HasErrors = error != null;
            if (this.hasErrors)
            {
                List<string> errors = new List<string>();
                ErrorFormatter.FormatExceptionMessage(error, errors);
                StringBuilder sb = new StringBuilder();
                foreach (string errorLine in errors)
                {
                    sb.AppendLine(errorLine);
                }

                this.ErrorMessage = sb.ToString();
            }
        }


        public void Refresh(PublishProfileRefreshTarget refreshTarget)
        {
            Task.Run(async () =>
            {
                this.EnterRefresh();

                switch (refreshTarget)
                {
                    case PublishProfileRefreshTarget.Client:
                        await this.RefreshClient();
                        break;
                    case PublishProfileRefreshTarget.Organizations:
                        await this.RefreshOrganizations();
                        break;
                    case PublishProfileRefreshTarget.Spaces:
                        await this.RefreshSpaces();
                        break;
                    case PublishProfileRefreshTarget.ServiceInstances:
                        await this.RefreshServiceInstances();
                        break;
                    case PublishProfileRefreshTarget.Stacks:
                        await this.RefreshStacks();
                        break;
                    case PublishProfileRefreshTarget.Buildpacks:
                        await this.RefreshBuildpacks();
                        break;
                    case PublishProfileRefreshTarget.SharedDomains:
                        await this.RefreshSharedDomains();
                        break;
                    case PublishProfileRefreshTarget.PrivateDomains:
                        await this.RefreshPrivateDomains();
                        break;
                    default:
                        break;
                }
            }).ContinueWith((antecedent) => 
            {
                if (antecedent.Exception != null)
                {
                    this.ExitRefresh(antecedent.Exception);
                }
                else
                {
                    this.ExitRefresh();
                }
            }).Forget();
        }

        private async Task RefreshClient()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Client;

            this.client = new CloudFoundryClient(
                this.publishProfile.ServerUri,
                this.cancellationToken,
                null,
                this.publishProfile.SkipSSLValidation);

            string password = CloudCredentialsManager.GetPassword(
                this.publishProfile.ServerUri,
                this.publishProfile.User);

            var authenticationContext = await client.Login(new CloudCredentials()
            {
                User = this.publishProfile.User,
                Password = password
            });

            await this.RefreshOrganizations();
            await this.RefreshStacks();
            await this.RefreshBuildpacks();
            await this.RefreshSharedDomains();
        }

        private async Task RefreshOrganizations()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Organizations;

            PagedResponseCollection<ListAllOrganizationsResponse> orgs = await client.Organizations.ListAllOrganizations();

            while (orgs != null && orgs.Properties.TotalResults != 0)
            {
                foreach (var org in orgs)
                {
                    this.orgs.Add(org);
                }

                orgs = await orgs.GetNextPage();
            }

            await this.RefreshSpaces();
            await this.RefreshPrivateDomains();
        }

        private async Task RefreshSpaces()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Spaces;

            this.spaces.Clear();

            var org = this.orgs.FirstOrDefault(o => o.Name == this.publishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await this.client.Organizations.ListAllSpacesForOrganization(org.EntityMetadata.Guid);

            while (spaces != null && spaces.Properties.TotalResults != 0)
            {
                foreach (var space in spaces)
                {
                    this.spaces.Add(space);
                }

                spaces = await spaces.GetNextPage();
            }

            await this.RefreshServiceInstances();
        }

        private async Task RefreshServiceInstances()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.ServiceInstances;

            this.serviceInstances.Clear();

            var space = this.spaces.FirstOrDefault(s => s.Name == this.publishProfile.Space);

            if (space == null)
            {
                return;
            }

            PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> serviceInstances = await this.client.Spaces.ListAllServiceInstancesForSpace(space.EntityMetadata.Guid);

            while (serviceInstances != null && serviceInstances.Properties.TotalResults != 0)
            {
                foreach (var privateDomain in serviceInstances)
                {
                    this.serviceInstances.Add(privateDomain);
                }

                serviceInstances = await serviceInstances.GetNextPage();
            }
        }

        private async Task RefreshStacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Stacks;

            this.stacks.Clear();

            PagedResponseCollection<ListAllStacksResponse> stacks = await this.client.Stacks.ListAllStacks();

            while (stacks != null && stacks.Properties.TotalResults != 0)
            {
                foreach (var stack in stacks)
                {
                    this.stacks.Add(stack);
                }

                stacks = await stacks.GetNextPage();
            }
        }

        private async Task RefreshBuildpacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Buildpacks;

            this.buildpacks.Clear();

            PagedResponseCollection<ListAllBuildpacksResponse> buildpacks = await this.client.Buildpacks.ListAllBuildpacks();

            while (buildpacks != null && buildpacks.Properties.TotalResults != 0)
            {
                foreach (var buildpack in buildpacks)
                {
                    this.buildpacks.Add(buildpack);
                }

                buildpacks = await buildpacks.GetNextPage();
            }
        }

        private async Task RefreshSharedDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.SharedDomains;

            this.sharedDomains.Clear();

            PagedResponseCollection<ListAllSharedDomainsResponse> sharedDomains = await this.client.SharedDomains.ListAllSharedDomains();

            while (sharedDomains != null && sharedDomains.Properties.TotalResults != 0)
            {
                foreach (var sharedDomain in sharedDomains)
                {
                    this.sharedDomains.Add(sharedDomain);
                }

                sharedDomains = await sharedDomains.GetNextPage();
            }
        }

        private async Task RefreshPrivateDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.PrivateDomains;

            this.privateDomains.Clear();

            var org = this.orgs.FirstOrDefault(o => o.Name == this.publishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = await this.client.Organizations.ListAllPrivateDomainsForOrganization(org.EntityMetadata.Guid);

            while (privateDomains != null && privateDomains.Properties.TotalResults != 0)
            {
                foreach (var privateDomain in privateDomains)
                {
                    this.privateDomains.Add(privateDomain);
                }

                privateDomains = await privateDomains.GetNextPage();
            }
        }
    }
}
