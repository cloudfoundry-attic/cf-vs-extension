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

    [ComVisible(true)]
    [Serializable, XmlRoot("PropertyGroup")]
    public class PublishProfile
    {
        private string configFile = string.Empty;

        private string username = string.Empty;
        private string password = string.Empty;
        private string server = string.Empty;
        private string organization = string.Empty;
        private string space = string.Empty;
        private string deploytargetfile = string.Empty;
        private string webpublishmethod = "CloudFoundry";
        private string refreshToken = string.Empty;
        private bool savedPassword = true;
        private bool skipSSLValidation = true;
        private CloudFoundryClient cfClient;
        private Application appManifest = new Application();
        private BusyBox busyBox = new BusyBox();

        private ObservableCollection<ListAllSpacesForOrganizationResponse> spaces = new ObservableCollection<ListAllSpacesForOrganizationResponse>();

        private ObservableCollection<ListAllOrganizationsResponse> orgs = new ObservableCollection<ListAllOrganizationsResponse>();

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

        public string ConfigFile
        {
            get { return this.configFile; }
        }

        public string CFUser
        {
            get { return this.username; }
            set { this.username = value; }
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

        public string DeployTargetFile
        {
            get { return this.deploytargetfile; }
            set { this.deploytargetfile = value; }
        }

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

        public void Initialize(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
        }

        public async Task InitiCFClient()
        {
            this.busyBox.IsBusy = true;
            this.busyBox.BusyMessage = "Initializing client";
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
            this.busyBox.IsBusy = false;
        }


        public static PublishProfile LoadFromFile(string filePath)
        {
            PublishProfile publishProfile = new PublishProfile();
            XmlSerializer serializer = new XmlSerializer(typeof(PublishProfile));

            using (XmlReader xmlReader = XmlReader.Create(filePath))
            {
                xmlReader.ReadToDescendant("PropertyGroup");
                publishProfile = (PublishProfile)serializer.Deserialize(xmlReader.ReadSubtree());
            }


            CloudFoundry.Manifests.ManifestDiskRepository manifestRepo = new ManifestDiskRepository();
            var manifest = manifestRepo.ReadManifest(publishProfile.deploytargetfile);

            if (manifest.Applications().Count() != 1)
            {
                throw new FileFormatException("Invalid publish file, Cloud Foundry manifest must contain only one application.");
            }
            publishProfile.appManifest = manifest.Applications().First();
            publishProfile.configFile = filePath;
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
            this.busyBox.IsBusy = true;
            this.busyBox.BusyMessage = "Loading organizations";
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
            this.busyBox.IsBusy = true;
            this.busyBox.BusyMessage = "Loading spaces";
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
