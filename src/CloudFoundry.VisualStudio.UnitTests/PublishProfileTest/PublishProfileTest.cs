using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.VisualStudio;
using CloudFoundry.VisualStudio.ProjectPush;
using System.IO;
using EnvDTE;
using System.Collections.Generic;

namespace CloudFoundry.VisualStudio.UnitTests.PublishProfileTest
{
    [TestClass()]
    [DeploymentItem("Assets")]
    public class PublishProfile
    {
        private string PublishProfileProjectDir
        {
            get
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string projectPath = Path.Combine(assemblyPath, "project");
                return projectPath;
            }
        }

        private string PublishProfilePath
        {
            get
            {
                string profilePath = Path.Combine(this.PublishProfileProjectDir, "Properties", "PublishProfiles", "push.cf.pubxml");
                return profilePath;
            }
        }

        [TestMethod()]
        public void LoadExistingProfileTest()
        {
            // Arrange
            Project project = new ProjectMock(PublishProfileProjectDir);

            // Act
            PublishProfile2 publishProfile = PublishProfile2.Load(project, this.PublishProfilePath);

            // Assert
            Assert.AreEqual("user", publishProfile.CFUser);
            Assert.AreEqual(string.Empty, publishProfile.CFPassword);
            Assert.AreEqual(string.Empty, publishProfile.CFRefreshToken);
            Assert.AreEqual("https://api.1.2.3.4.xip.io/", publishProfile.CFServerUri);
            Assert.AreEqual(true, publishProfile.CFSkipSSLValidation);
            Assert.AreEqual("numenor", publishProfile.CFOrganization);
            Assert.AreEqual("arandor", publishProfile.CFSpace);
            Assert.AreEqual(string.Empty, publishProfile.DeployTargetFile);
            Assert.AreEqual("CloudFoundry", publishProfile.WebPublishMethod);
            Assert.AreEqual("manifest.yml", publishProfile.CFManifest);
        }

        [TestMethod()]
        public void LoadExistingManifestTest()
        {
            // Arrange
            Project project = new ProjectMock(PublishProfileProjectDir);

            // Act
            PublishProfile2 publishProfile = PublishProfile2.Load(project, this.PublishProfilePath);

            // Assert
            Assert.AreEqual("test-bp", publishProfile.Application.BuildpackUrl);
            Assert.AreEqual("cmd", publishProfile.Application.Command);
            Assert.AreEqual(null, publishProfile.Application.DiskQuota);
            Assert.AreEqual("app.example.com", publishProfile.Application.Domains[0]);
            Assert.AreEqual("first", publishProfile.Application.EnvironmentVars["env1"]);
            Assert.AreEqual("second", publishProfile.Application.EnvironmentVars["env2"]);
            Assert.AreEqual(500, publishProfile.Application.HealthCheckTimeout);
            Assert.AreEqual("home", publishProfile.Application.Hosts[0]);
            Assert.AreEqual(1, publishProfile.Application.InstanceCount);
            Assert.AreEqual(128, publishProfile.Application.Memory);
            Assert.AreEqual("app-name", publishProfile.Application.Name);
            Assert.AreEqual(false, publishProfile.Application.NoHostname);
            Assert.AreEqual(false, publishProfile.Application.NoRoute);
            Assert.AreEqual(@"c:\path\to\app", publishProfile.Application.Path);
            Assert.AreEqual("mysql", publishProfile.Application.ServicesToBind[0]);
            Assert.AreEqual("mssql", publishProfile.Application.ServicesToBind[1]);
            Assert.AreEqual(null, publishProfile.Application.StackName);
            Assert.AreEqual(false, publishProfile.Application.UseRandomHostname);
        }

        [TestMethod()]
        public void LoadNonExistingProfile()
        {
            // Arrange
            Project project = new ProjectMock(@"c:\somedirthatdoesntexist");

            // Act
            PublishProfile2 publishProfile = PublishProfile2.Load(project, @"c:\foo-bar.cf.pubxml");

            // Assert
            Assert.AreEqual(string.Empty, publishProfile.CFUser);
            Assert.AreEqual(null, publishProfile.CFPassword);
            Assert.AreEqual(null, publishProfile.CFRefreshToken);
            Assert.AreEqual(string.Empty, publishProfile.CFServerUri);
            Assert.AreEqual(false, publishProfile.CFSkipSSLValidation);
            Assert.AreEqual(string.Empty, publishProfile.CFOrganization);
            Assert.AreEqual(string.Empty, publishProfile.CFSpace);
            Assert.AreEqual(null, publishProfile.DeployTargetFile);
            Assert.AreEqual("CloudFoundry", publishProfile.WebPublishMethod);
            Assert.AreEqual("manifest.yml", publishProfile.CFManifest);
        }

        [TestMethod()]
        public void LoadNonExistingManifest()
        {
            // Arrange
            Project project = new ProjectMock(@"c:\somedirthatdoesntexist");

            // Act
            PublishProfile2 publishProfile = PublishProfile2.Load(project, @"c:\foo-bar.cf.pubxml");

            // Assert
            Assert.AreEqual(string.Empty, publishProfile.Application.BuildpackUrl);
            Assert.AreEqual(null, publishProfile.Application.Command);
            Assert.AreEqual(null, publishProfile.Application.DiskQuota);
            Assert.AreEqual(0, publishProfile.Application.Domains.Length);
            Assert.AreEqual(0, publishProfile.Application.EnvironmentVars.Count);
            Assert.AreEqual(null, publishProfile.Application.HealthCheckTimeout);
            Assert.AreEqual(project.Name.ToLowerInvariant(), publishProfile.Application.Hosts[0]);
            Assert.AreEqual(1, publishProfile.Application.InstanceCount);
            Assert.AreEqual(256, publishProfile.Application.Memory);
            Assert.AreEqual(project.Name, publishProfile.Application.Name);
            Assert.AreEqual(false, publishProfile.Application.NoHostname);
            Assert.AreEqual(false, publishProfile.Application.NoRoute);
            Assert.AreEqual(null, publishProfile.Application.Path);
            Assert.AreEqual(0, publishProfile.Application.ServicesToBind.Length);
            Assert.AreEqual(string.Empty, publishProfile.Application.StackName);
            Assert.AreEqual(false, publishProfile.Application.UseRandomHostname);
        }
    }
}
