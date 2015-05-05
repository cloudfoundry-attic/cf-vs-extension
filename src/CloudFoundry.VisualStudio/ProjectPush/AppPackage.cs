using CloudFoundry.VisualStudio.Forms;
using EnvDTE;
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

namespace CloudFoundry.VisualStudio.ProjectPush
{
    [ComVisible(true)]
    public class AppPackage
    {
        private string configFile = string.Empty;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _server = string.Empty;
        private string _appname = string.Empty;
        private string _organization = string.Empty;
        private string _space = string.Empty;
        private int _memory = 512;
        private int _instances = 1;
        private string _stack = string.Empty;
        private string _routes = string.Empty;
        private string _manifestpath = string.Empty;
        private string _deploytargetfile = string.Empty;
        private bool _localbuild = true;
        private string _webpublishmethod = "CloudFoundry";
        private string _configuration = string.Empty;
        private string _platform = string.Empty;
        private string _services = string.Empty;
        private string _refreshToken = string.Empty;
        private bool _savedPassword = true;
        private bool _skipSSLValidation = true;

        public string ConfigFile { get { return configFile; } }

        public string CFUser { get { return _username; } set { _username = value; } }

        public string CFPassword { get { return _password; } set { _password = value; } }

        public bool CFSavedPassword { get { return _savedPassword; } set { _savedPassword = value; } }

        public string CFRefreshToken { get { return _refreshToken; } set { _refreshToken = value; } }

        public string CFServerUri { get { return _server; } set { _server = value; } }

        public string CFAppName { get { return _appname; } set { _appname = value; } }

        public string CFOrganization { get { return _organization; } set { _organization = value; } }

        public string CFSpace { get { return _space; } set { _space = value; } }

        public int CFAppMemory { get { return _memory; } set { _memory = value; } }

        public int CFAppInstances { get { return _instances; } set { _instances = value; } }

        public string CFStack { get { return _stack; } set { _stack = value; } }

        public string CFRoutes { get { return _routes; } set { _routes = value; } }

        public string CFServices { get { return _services; } set { _services = value; } }
        public string CFConfigurationFile { get { return _manifestpath; } set { _manifestpath = value; } }

        public string DeployTargetFile { get { return _deploytargetfile; } set { _deploytargetfile = value; } }

        public bool CFLocalBuild { get { return _localbuild; } set { _localbuild = value; } }

        public string WebPublishMethod { get { return _webpublishmethod; } set { _webpublishmethod = value; } }

        public string CFMSBuildConfiguration { get { return _configuration; } set { _configuration = value; } }

        public string CFMSBuildPlatform { get { return _platform; } set { _platform = value; } }

        public bool CFSkipSSLValidation { get { return _skipSSLValidation; } set { _skipSSLValidation = value; } }

        public void Initialize(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            configFile = Path.Combine(Path.GetDirectoryName(project.FullName), "Properties", "PublishProfiles", string.Format(CultureInfo.InvariantCulture, "push{0}", CloudFoundry_VisualStudioPackage.extension));
            if (File.Exists(configFile))
            {

                LoadFromFile(configFile);

                if (CFAppName == string.Empty)
                {
                    CFAppName = project.Name;
                }

            }
        }

        public void LoadFromFile(string filePath)
        {
            configFile = filePath;
            XmlDocument doc = new XmlDocument();

            doc.Load(filePath);

            XmlNode properiesNode = doc.DocumentElement.FirstChild;

            foreach (XmlNode node in properiesNode.ChildNodes)
            {
                switch (node.Name.ToLowerInvariant())
                {
                    case "cfuser": { CFUser = node.InnerText; break; }
                    case "cfpassword": { CFPassword = node.InnerText; break; }
                    case "cfrefreshtoken": { CFRefreshToken = node.InnerText; break; }
                    case "cfsavedpassword": { if (node.InnerText == string.Empty) { CFSavedPassword = false; } else { CFSavedPassword = Convert.ToBoolean(node.InnerText); } break; }
                    case "cfskipsslvalidation": { if (node.InnerText == string.Empty) { CFSkipSSLValidation = false; } else { CFSkipSSLValidation = Convert.ToBoolean(node.InnerText); } break; }
                    case "cfserveruri": { CFServerUri = node.InnerText; break; }
                    case "cforganization": { CFOrganization = node.InnerText; break; }
                    case "cfspace": { CFSpace = node.InnerText; break; }
                    case "cfappname": { CFAppName = node.InnerText; break; }
                    case "cfappmemory": { CFAppMemory = Convert.ToInt32(node.InnerText); break; }
                    case "cfappinstances": { CFAppInstances = Convert.ToInt32(node.InnerText); break; }
                    case "cfstack": { CFStack = node.InnerText; break; }
                    case "cfroutes": { CFRoutes = node.InnerText; break; }
                    case "cfconfigurationfile": { CFConfigurationFile = node.InnerText; break; }
                    case "cfservices": { CFServices = node.InnerText; break; }
                    case "cflocalbuild": { CFLocalBuild = Convert.ToBoolean(node.InnerText); break; }
                    case "deploytargetfile": { DeployTargetFile = node.InnerText; break; }
                    case "webpublishmethod": { WebPublishMethod = node.InnerText; break; }
                    case "cfmsbuildconfiguration": { CFMSBuildConfiguration = node.InnerText; break; }
                    case "cfmsbuildplatform": { CFMSBuildPlatform = node.InnerText; break; }
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

            content = content.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?><AppPackage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">", "").Replace("</AppPackage>", "");

            content += "</PropertyGroup></Project>";


            File.WriteAllText(filePath, content);
        }

        internal void Save()
        {
            SaveToFile(configFile);
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
