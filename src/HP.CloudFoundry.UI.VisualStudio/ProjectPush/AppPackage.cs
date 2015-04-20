using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HP.CloudFoundry.UI.VisualStudio.ProjectPush
{
    [ComVisible(true)]
    public class AppPackage  
    {
        private string configFile = string.Empty;

        private string _username;
        private string _password;
        private string _server;
        private string _appname;
        private string _organization;
        private string _space;
        private int _memory;
        private int _instances;
        private string _stack;
        private string _routes;
        private string _manifestpath;
        private string _deploytargetfile;
        private bool _localbuild;
        private string _webpublishmethod;
        private string _configuration;
        private string _platform;
        private string _services;

        [DisplayName("User name")]
        [Category("Cloud Foundry")]
        [DefaultValue("")]
        public string CFUser { get { return _username; } set { _username = value; SaveToFile(configFile); } }
        [DisplayName("Password")]
        [Category("Cloud Foundry")]
        [DefaultValue("")]
        [Browsable(false)]
        public string CFPassword { get { return _password; } set { _password = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFServerUri { get { return _server; } set { _server = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFAppName { get { return _appname; } set { _appname = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFOrganization { get { return _organization; } set { _organization = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFSpace { get { return _space; } set { _space = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public int CFMemory { get { return _memory; } set { _memory = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public int CFInstancesNumber { get { return _instances; } set { _instances = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFStack { get { return _stack; } set { _stack = value; SaveToFile(configFile); } }
        [DisplayName("Route")]
        [Category("Cloud Foundry")]
        public string CFRoutes { get { return _routes; } set { _routes = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFServices { get { return _services; } set { _services = value; SaveToFile(configFile); } }
        public string CFConfigurationFile { get { return _manifestpath; } set { _manifestpath = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string DeployTargetFile { get { return _deploytargetfile; } set { _deploytargetfile = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public bool CFLocalBuild { get { return _localbuild; } set { _localbuild = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        [Browsable(false)]
        public string WebPublishMethod { get { return _webpublishmethod; } set { _webpublishmethod = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFMSBuildConfiguration { get { return _configuration; } set { _configuration = value; SaveToFile(configFile); } }
        [Category("Cloud Foundry")]
        public string CFMSBuildPlatform { get { return _platform; } set { _platform = value; SaveToFile(configFile); } }

        public void Initialize(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            configFile = Path.Combine(Path.GetDirectoryName(project.FullName),"Properties","PublishProfiles","push.cf.pubxml");
            if (File.Exists(configFile))
            {
                LoadFromFile(configFile);
                if (CFAppName == "test")
                {
                    CFAppName = project.Name;
                }
                if (CFConfigurationFile == "C:\\test\\manifest.yaml")
                {
                    CFConfigurationFile = Path.Combine(Path.GetDirectoryName(project.FullName), "manifest.yaml");
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
                    case "cfserveruri": { CFServerUri = node.InnerText; break; }
                    case "cforganization": { CFOrganization = node.InnerText; break; }
                    case "cfspace": { CFSpace = node.InnerText; break; }
                    case "cfappname": { CFAppName = node.InnerText; break; }
                    case "cfmemory": { CFMemory = Convert.ToInt32(node.InnerText); break; }
                    case "cfinstancesnumber": { CFInstancesNumber = Convert.ToInt32(node.InnerText); break; }
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
    }
}
