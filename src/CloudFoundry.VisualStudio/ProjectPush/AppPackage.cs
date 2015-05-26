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

    [ComVisible(true)]
    public class AppPackage
    {
        private string configFile = string.Empty;

        private string username = string.Empty;
        private string password = string.Empty;
        private string server = string.Empty;
        private string appname = string.Empty;
        private string organization = string.Empty;
        private string space = string.Empty;
        private int memory = 512;
        private int instances = 1;
        private string stack = string.Empty;
        private string routes = string.Empty;
        private string manifestpath = string.Empty;
        private string deploytargetfile = string.Empty;
        private bool localbuild = true;
        private string webpublishmethod = "CloudFoundry";
        private string configuration = string.Empty;
        private string platform = string.Empty;
        private string services = string.Empty;
        private string refreshToken = string.Empty;
        private bool savedPassword = true;
        private bool skipSSLValidation = true;

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

        public string CFAppName 
        {
            get { return this.appname; }
            set { this.appname = value; } 
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

        public int CFAppMemory
        {
            get { return this.memory; }
            set { this.memory = value; }
        }

        public int CFAppInstances 
        {
            get { return this.instances; }
            set { this.instances = value; }
        }

        public string CFStack 
        {
            get { return this.stack; } 
            set { this.stack = value; }
        }

        public string CFRoutes
        {
            get { return this.routes; }
            set { this.routes = value; }
        }

        public string CFServices 
        {
            get { return this.services; }
            set { this.services = value; }
        }

        public string CFConfigurationFile
        {
            get { return this.manifestpath; }
            set { this.manifestpath = value; }
        }

        public string DeployTargetFile 
        {
            get { return this.deploytargetfile; }
            set { this.deploytargetfile = value; }
        }

        public bool CFLocalBuild 
        {
            get { return this.localbuild; } 
            set { this.localbuild = value; }
        }

        public string WebPublishMethod 
        {
            get { return this.webpublishmethod; } 
            set { this.webpublishmethod = value; }
        }

        public string CFMSBuildConfiguration 
        {
            get { return this.configuration; }
            set { this.configuration = value; }
        }

        public string CFMSBuildPlatform 
        {
            get { return this.platform; } 
            set { this.platform = value; }
        }

        public bool CFSkipSSLValidation 
        {
            get { return this.skipSSLValidation; }
            set { this.skipSSLValidation = value; }
        }

        public void Initialize(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            this.CFAppName = project.Name;
        }

        public void LoadFromFile(string filePath)
        {
            this.configFile = filePath;
            XmlDocument doc = new XmlDocument();

            doc.Load(filePath);

            XmlNode properiesNode = doc.DocumentElement.FirstChild;

            foreach (XmlNode node in properiesNode.ChildNodes)
            {
                switch (node.Name.ToLowerInvariant())
                {
                    case "cfuser": 
                        {
                            this.CFUser = node.InnerText; 
                            break;
                        }

                    case "cfpassword": 
                        { 
                            this.CFPassword = node.InnerText; 
                            break; 
                        }

                    case "cfrefreshtoken": 
                        { 
                            this.CFRefreshToken = node.InnerText; 
                            break; 
                        }

                    case "cfsavedpassword": 
                        {
                            if (node.InnerText == string.Empty) 
                            {
                                this.CFSavedPassword = false; 
                            } 
                            else 
                            { 
                                this.CFSavedPassword = Convert.ToBoolean(node.InnerText);
                            }

                            break;
                        }

                    case "cfskipsslvalidation":
                        {
                            if (node.InnerText == string.Empty)
                            {
                                this.CFSkipSSLValidation = false;
                            }
                            else
                            {
                                this.CFSkipSSLValidation = Convert.ToBoolean(node.InnerText);
                            }

                            break;
                        }

                    case "cfserveruri": 
                        {
                            this.CFServerUri = node.InnerText;
                            break;
                        }

                    case "cforganization":
                        {
                            this.CFOrganization = node.InnerText;
                            break;
                        }

                    case "cfspace": 
                        {
                            this.CFSpace = node.InnerText;
                            break; 
                        }

                    case "cfappname":
                        {
                            this.CFAppName = node.InnerText;
                            break;
                        }

                    case "cfappmemory":
                        {
                            this.CFAppMemory = Convert.ToInt32(node.InnerText);
                            break;
                        }

                    case "cfappinstances": 
                        {
                            this.CFAppInstances = Convert.ToInt32(node.InnerText);
                            break;
                        }

                    case "cfstack":
                        {
                            this.CFStack = node.InnerText; 
                            break;
                        }

                    case "cfroutes": 
                        {
                            this.CFRoutes = node.InnerText;
                            break;
                        }

                    case "cfconfigurationfile": 
                        {
                            this.CFConfigurationFile = node.InnerText; 
                            break;
                        }

                    case "cfservices":
                        {
                            this.CFServices = node.InnerText; 
                            break; 
                        }

                    case "cflocalbuild":
                        {
                            this.CFLocalBuild = Convert.ToBoolean(node.InnerText);
                            break; 
                        }

                    case "deploytargetfile": 
                        {
                            this.DeployTargetFile = node.InnerText;
                            break;
                        }

                    case "webpublishmethod":
                        {
                            this.WebPublishMethod = node.InnerText;
                            break;
                        }

                    case "cfmsbuildconfiguration": 
                        {
                            this.CFMSBuildConfiguration = node.InnerText;
                            break;
                        }

                    case "cfmsbuildplatform": 
                        {
                            this.CFMSBuildPlatform = node.InnerText;
                            break;
                        }

                    default: break;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "xmlWriter does not take ownership over textWriter")]
        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppPackage));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            string content = "<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup>";

            using (StringWriter textWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, this);
                }

                content += textWriter.ToString();
            }

            content = content.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?><AppPackage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">", string.Empty).Replace("</AppPackage>", string.Empty);

            content += "</PropertyGroup></Project>";

            this.configFile = filePath;

            File.WriteAllText(filePath, content);
        }

        internal bool IsEqualTo(AppPackage that)
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
