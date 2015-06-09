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

    /// <summary>
    /// An EventArgs class for error events.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error that occured while trying to communicate with the Loggregator service.
        /// </summary>
        /// <value>
        /// An exception that describes the error.
        /// </value>
        public Exception Error
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets a formatted error message that can be shown to the user.
        /// <remarks>The error message can contain newline characters.</remarks>
        /// </summary>
        /// <value>
        /// The formatted error.
        /// </value>
        public string FormattedError
        {
            get;
            internal set;
        }
    }

    [ComVisible(true)]
    [Serializable, XmlRoot("PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class PublishProfile2 : INotifyPropertyChanged
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

        [XmlElement(ElementName="CFUser")]
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

        private PublishProfile2()
        {
            this.namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] {
                new XmlQualifiedName(string.Empty, "http://schemas.microsoft.com/developer/msbuild/2003")
            });
        }

        private void LoadManifest()
        {
            string projectDir = Path.GetDirectoryName(project.FullName);
            string absoluteManifestPath = Path.Combine(projectDir, this.Manifest);


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
            string projectDir = Path.GetDirectoryName(project.FullName);
            Directory.CreateDirectory(projectDir);

            string absoluteManifestPath = Path.Combine(projectDir, this.Manifest);

            CloudFoundry.Manifests.Manifest.Save(new Application[] { this.Application }, absoluteManifestPath);
        }

        /// <summary>
        /// Loads the specified publish profile for the specified project.
        /// </summary>
        /// <param name="project">The Visual Studio EnvDTE project. Cannot be null.</param>
        /// <param name="path">Absolute path to the publish profile to load. If the file does not exist, defaults will be loaded for the object.</param>
        /// <returns>A new PublishProfile.</returns>
        /// <exception cref="System.ArgumentNullException">project</exception>
        public static PublishProfile2 Load(Project project, string path)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            PublishProfile2 publishProfile = null;

            if (File.Exists(path))
            {
                // If the file exists, we load it
                XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile2));

                using (XmlReader xmlReader = XmlReader.Create(path))
                {
                    xmlReader.ReadToDescendant("PropertyGroup");
                    publishProfile = (PublishProfile2)serializer.Deserialize(xmlReader.ReadSubtree());
                }
            }
            else
            {
                // If the file does not exist, we set defaults
                publishProfile = new PublishProfile2()
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

        public void Save()
        {
            SaveManifest();

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile2),
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



    internal class PublishProfileEditorResources
    {
        private ObservableCollection<ListAllOrganizationsResponse> orgs = new ObservableCollection<ListAllOrganizationsResponse>();
        private ObservableCollection<ListAllSpacesForOrganizationResponse> spaces = new ObservableCollection<ListAllSpacesForOrganizationResponse>();
        private ObservableCollection<ListAllStacksResponse> stacks = new ObservableCollection<ListAllStacksResponse>();
        private ObservableCollection<ListAllBuildpacksResponse> buildpacks = new ObservableCollection<ListAllBuildpacksResponse>();
        private ObservableCollection<ListAllSharedDomainsResponse> sharedDomains = new ObservableCollection<ListAllSharedDomainsResponse>();
        private ObservableCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = new ObservableCollection<ListAllPrivateDomainsForOrganizationResponse>();
        private ObservableCollection<ListAllServiceInstancesForSpaceResponse> serviceInstances = new ObservableCollection<ListAllServiceInstancesForSpaceResponse>();


        public PublishProfile2 publishProfile;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

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

        public PublishProfileEditorResources(PublishProfile2 publishProfile, CancellationToken cancellationToken)
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

        public async Task RefreshClient()
        {
            try
            {
                EnterRefresh();

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
            catch (Exception ex)
            {
                ExitRefresh(ex);
            }
        }

        public async Task RefreshOrganizations()
        {
            this.orgs.Clear();

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

        public async Task RefreshSpaces()
        {
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

        public async Task RefreshServiceInstances()
        {
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

        public async Task RefreshStacks()
        {
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

        public async Task RefreshBuildpacks()
        {
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

        public async Task RefreshSharedDomains()
        {
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

        public async Task RefreshPrivateDomains()
        {
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

        private void NotifyError(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (ErrorOccurred == null)
            {
                return;
            }

            List<string> errors = new List<string>();
            ErrorFormatter.FormatExceptionMessage(error, errors);
            StringBuilder sb = new StringBuilder();
            foreach (string errorLine in errors)
            {
                sb.AppendLine(errorLine);
            }

            ErrorEventArgs errorArgs = new ErrorEventArgs();
            errorArgs.FormattedError = sb.ToString();
            errorArgs.Error = error;

            ErrorOccurred(this, errorArgs);
        }
    }


    [ComVisible(true)]
    [Serializable, XmlRoot("PropertyGroup")]
    public class PublishProfile
    {
        private string username = string.Empty;
        private string password = string.Empty;
        private string server = string.Empty;
        private string organization = string.Empty;
        private string space = string.Empty;
        //private string deploytargetfile = string.Empty;
        private string webpublishmethod = "CloudFoundry";
        private string refreshToken = string.Empty;
        private bool savedPassword = true;
        private bool skipSSLValidation = true;
        private CloudFoundryClient cfClient;
        private Application appManifest = new Application();
        private BusyBox busyBox = new BusyBox();


        private ObservableCollection<ListAllSpacesForOrganizationResponse> spaces = new ObservableCollection<ListAllSpacesForOrganizationResponse>();

        private ObservableCollection<ListAllOrganizationsResponse> orgs = new ObservableCollection<ListAllOrganizationsResponse>();
        private ObservableCollection<ListAllStacksResponse> stacks = new ObservableCollection<ListAllStacksResponse>();
        private ObservableCollection<ListAllBuildpacksResponse> buildpacks = new ObservableCollection<ListAllBuildpacksResponse>();
        private string path;

        [XmlIgnore]
        public ObservableCollection<ListAllBuildpacksResponse> Buildpacks
        {
            get { return buildpacks; }
        }

        [XmlIgnore]
        public ObservableCollection<ListAllStacksResponse> Stacks
        {
            get { return stacks; }
        }

        [XmlIgnore]
        public BusyBox BusyBox
        {
            get { return busyBox; }
        }

        [XmlIgnore]
        public ObservableCollection<ListAllSpacesForOrganizationResponse> Spaces
        {
            get { return spaces; }
        }

        [XmlIgnore]
        public ObservableCollection<ListAllOrganizationsResponse> Orgs
        {
            get { return orgs; }
        }

        public string CFUser
        {
            get { return this.username; }
            set { this.username = value; }
        }

        public string CFManifest
        {
            get;
            set;
        }

        public string CFPassword
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public bool CFSavedPassword
        {
            get { return this.savedPassword; }
            set { this.savedPassword = value; }
        }

        public string CFRefreshToken
        {
            get { return this.refreshToken; }
            set { this.refreshToken = value; }
        }

        public string CFServerUri
        {
            get { return this.server; }
            set { this.server = value; }
        }

        public string CFOrganization
        {
            get { return this.organization; }
            set { this.organization = value; }
        }

        public string CFSpace
        {
            get { return this.space; }
            set { this.space = value; }
        }

        //public string DeployTargetFile
        //{
        //    get { return this.deploytargetfile; }
        //    set { this.deploytargetfile = value; }
        //}

        public string WebPublishMethod
        {
            get { return this.webpublishmethod; }
            set { this.webpublishmethod = value; }
        }

        public bool CFSkipSSLValidation
        {
            get { return this.skipSSLValidation; }
            set { this.skipSSLValidation = value; }
        }

        public Application CFAppManifest
        {
            get { return appManifest; }
        }

        //public  Initialize(Project project)
        //{
        //    if (project == null)
        //    {
        //        throw new ArgumentNullException("project");
        //    }
        //}

        private PublishProfile()
        {

        }

        public async Task InitiCFClient()
        {
            this.busyBox.SetMessage("Initializing client");
            this.cfClient = new CloudFoundryClient(new Uri(this.server), new System.Threading.CancellationToken(), null, this.skipSSLValidation);

            if (this.CFRefreshToken != string.Empty)
            {
                try
                {
                    await this.cfClient.Login(this.CFRefreshToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                if (this.CFUser == string.Empty)
                {
                    //imageType = "warning";
                    //message = "Please configure credentials for your target.";
                    //this.SetStatusInfo(imageType, message);
                    //this.DisablePublishButton();
                    return;
                }

                if (this.CFPassword == string.Empty)
                {
                    if (this.CFSavedPassword == true)
                    {
                        string password = CloudCredentialsManager.GetPassword(new Uri(this.CFServerUri), this.CFUser);
                        if (password == null)
                        {
                            //imageType = "warning";
                            //message = "Please configure credentials for your target.";
                            //this.SetStatusInfo(imageType, message);
                            //this.DisablePublishButton();
                            return;
                        }

                        CloudCredentials creds = new CloudCredentials();
                        creds.User = this.CFUser;
                        creds.Password = password;
                        try
                        {
                            await this.cfClient.Login(creds).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            //imageType = "error";
                            //message = ex.Message;
                            //this.SetStatusInfo(imageType, message);
                            //this.DisablePublishButton();
                            throw ex;
                        }

                        //imageType = "ok";
                        //message = "Target configuration is valid";
                    }
                    else
                    {
                        //imageType = "warning";
                        //message = "Please configure credentials for your target.";
                        //this.SetStatusInfo(imageType, message);
                        //this.DisablePublishButton();
                        return;
                    }
                }
                else
                {
                    CloudCredentials creds = new CloudCredentials();
                    creds.User = this.CFUser;
                    creds.Password = this.CFPassword;
                    try
                    {
                        await this.cfClient.Login(creds);
                    }
                    catch (Exception ex)
                    {
                        //imageType = "error";
                        //message = string.Format(CultureInfo.InvariantCulture, "{0}. Your password is saved in clear text in the profile!", ex.Message);
                        //this.SetStatusInfo(imageType, message);
                        //this.DisablePublishButton();
                        throw ex;
                    }

                    //imageType = "warning";
                    //message = "Target login was successful, but your password is saved in clear text in profile!";
                }
            }
            await RefreshOrgs();
            await RefreshStacks();
            await RefreshBuildpacks();
            this.busyBox.IsBusy = false;
        }


        public static PublishProfile Initialize(string filePath)
        {
            PublishProfile publishProfile = new PublishProfile();
            XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile));

            using (XmlReader xmlReader = XmlReader.Create(filePath))
            {
                xmlReader.ReadToDescendant("PropertyGroup");
                publishProfile = (PublishProfile)serializer.Deserialize(xmlReader.ReadSubtree());
            }

            CloudFoundry.Manifests.ManifestDiskRepository manifestRepo = new ManifestDiskRepository();

            //string manifestPath = Proj

            var manifest = manifestRepo.ReadManifest(publishProfile.CFManifest);

            if (manifest.Applications().Count() != 1)
            {
                throw new FileFormatException("Invalid CloudFoundry manifest file, more than one application is configured.");
            }

            publishProfile.appManifest = manifest.Applications().First();
            publishProfile.path = filePath;
            return publishProfile;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "xmlWriter does not take ownership over textWriter")]
        public void SaveToFile(string filePath)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile));
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            //settings.Indent = false;
            //settings.OmitXmlDeclaration = false;

            //string content = "<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup>";

            //using (StringWriter textWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            //{
            //    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
            //    {
            //        serializer.Serialize(xmlWriter, this);
            //    }

            //    content += textWriter.ToString();
            //}

            //content = content.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?><PublishProfile xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">", string.Empty).Replace("</PublishProfile>", string.Empty);

            //content += "</PropertyGroup></Project>";

            //this.configFile = filePath;

            //File.WriteAllText(filePath, content);
        }

        internal async Task RefreshOrgs()
        {

            this.busyBox.SetMessage("Loading organizations");
            var srvOrgs = await cfClient.Organizations.ListAllOrganizations();

            this.orgs.Clear();
            foreach (var org in srvOrgs)
            {
                this.orgs.Add(org);
            }
            this.busyBox.IsBusy = false;

        }

        internal async Task RefreshSpaces()
        {
            this.busyBox.SetMessage("Loading spaces");
            this.spaces.Clear();

            if (this.orgs.Count == 0)
            {
                return;
            }

            foreach (var srvOrg in this.orgs)
            {
                if (srvOrg.Name == this.organization)
                {
                    var srvSpaces = await cfClient.Organizations.ListAllSpacesForOrganization(srvOrg.EntityMetadata.Guid);
                    foreach (var srvSpace in srvSpaces)
                    {
                        this.spaces.Add(srvSpace);
                    }
                    break;
                }
            }
            this.busyBox.IsBusy = false;

        }

        internal async Task RefreshStacks()
        {
            this.busyBox.SetMessage("Loading stacks");
            this.stacks.Clear();

            var srvStacks = await this.cfClient.Stacks.ListAllStacks();
            foreach (var srvStack in srvStacks)
            {
                this.stacks.Add(srvStack);
            }

            this.busyBox.IsBusy = false;
        }

        internal async Task RefreshBuildpacks()
        {
            this.busyBox.SetMessage("Loading Buildpacks");

            this.buildpacks.Clear();

            var srvBuildpacks = await this.cfClient.Buildpacks.ListAllBuildpacks();
            foreach (var srvBuildpack in srvBuildpacks)
            {
                this.buildpacks.Add(srvBuildpack);
            }

            this.busyBox.IsBusy = false;
        }

        internal bool IsEqualTo(PublishProfile that)
        {
            Type sourceType = this.GetType();
            Type destinationType = that.GetType();

            if (that == null)
            {
                return false;
            }

            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            foreach (PropertyInfo propertyInfo in sourceProperties)
            {
                var thisProperty = sourceType.GetProperty(propertyInfo.Name).GetValue(this, null);
                var thatProperty = destinationType.GetProperty(propertyInfo.Name).GetValue(that, null);

                if (thisProperty == null && thatProperty == null)
                {
                    continue;
                }

                if (thisProperty == null || thatProperty == null)
                {
                    return false;
                }

                if (thisProperty.ToString() != thatProperty.ToString())
                {
                    return false;
                }
            }

            return true;
        }



    }
}
