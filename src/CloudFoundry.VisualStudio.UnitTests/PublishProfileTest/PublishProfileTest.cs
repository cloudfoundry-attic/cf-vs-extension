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
    public class PublishProfileTest
    {
       

        [TestMethod()]
        public void LoadExistingProfileTest()
        {
            // Arrange
            Project project = new ProjectMock(Util.PublishProfileProjectDir);

            // Act
            PublishProfile publishProfile = PublishProfile.Load(project, Util.PublishProfilePath);

            // Assert
            Assert.AreEqual("user", publishProfile.User);
            Assert.AreEqual(string.Empty, publishProfile.Password);
            Assert.AreEqual(string.Empty, publishProfile.RefreshToken);
            Assert.AreEqual("https://api.1.2.3.4.xip.io/", publishProfile.ServerUri.ToString());
            Assert.AreEqual(true, publishProfile.SkipSSLValidation);
            Assert.AreEqual("numenor", publishProfile.Organization);
            Assert.AreEqual("arandor", publishProfile.Space);
            Assert.AreEqual(string.Empty, publishProfile.DeployTargetFile);
            Assert.AreEqual("CloudFoundry", publishProfile.WebPublishMethod);
            Assert.AreEqual("manifest.yml", publishProfile.Manifest);
        }

        [TestMethod()]
        public void LoadExistingManifestTest()
        {
            // Arrange
            Project project = new ProjectMock(Util.PublishProfileProjectDir);

            // Act
            PublishProfile publishProfile = PublishProfile.Load(project, Util.PublishProfilePath);

            // Assert
            Assert.AreEqual("test-bp", publishProfile.Application.BuildpackUrl);
            Assert.AreEqual("cmd", publishProfile.Application.Command);
            Assert.AreEqual(null, publishProfile.Application.DiskQuota);
            Assert.AreEqual("app.example.com", publishProfile.Application.Domains[0]);
            Assert.AreEqual("first", publishProfile.Application.EnvironmentVariables["env1"]);
            Assert.AreEqual("second", publishProfile.Application.EnvironmentVariables["env2"]);
            Assert.AreEqual(500, publishProfile.Application.HealthCheckTimeout);
            Assert.AreEqual("home", publishProfile.Application.Hosts[0]);
            Assert.AreEqual(1, publishProfile.Application.InstanceCount);
            Assert.AreEqual(128, publishProfile.Application.Memory);
            Assert.AreEqual("app-name", publishProfile.Application.Name);
            Assert.AreEqual(false, publishProfile.Application.NoHostName);
            Assert.AreEqual(false, publishProfile.Application.NoRoute);
            Assert.AreEqual(@"c:\path\to\app", publishProfile.Application.Path);
            Assert.AreEqual("mysql", publishProfile.Application.Services[0]);
            Assert.AreEqual("mssql", publishProfile.Application.Services[1]);
            Assert.AreEqual(null, publishProfile.Application.StackName);
            Assert.AreEqual(false, publishProfile.Application.UseRandomHostName);
        }

        [TestMethod()]
        public void LoadNonExistingProfile()
        {
            // Arrange
            Project project = new ProjectMock(@"c:\somedirthatdoesntexist");

            // Act
            PublishProfile publishProfile = PublishProfile.Load(project, @"c:\foo-bar.cf.pubxml");

            // Assert
            Assert.AreEqual(string.Empty, publishProfile.User);
            Assert.AreEqual(null, publishProfile.Password);
            Assert.AreEqual(null, publishProfile.RefreshToken);
            Assert.AreEqual(null, publishProfile.ServerUri);
            Assert.AreEqual(false, publishProfile.SkipSSLValidation);
            Assert.AreEqual(string.Empty, publishProfile.Organization);
            Assert.AreEqual(string.Empty, publishProfile.Space);
            Assert.AreEqual(null, publishProfile.DeployTargetFile);
            Assert.AreEqual("CloudFoundry", publishProfile.WebPublishMethod);
            Assert.AreEqual("manifest.yml", publishProfile.Manifest);
        }

        [TestMethod()]
        public void LoadNonExistingManifest()
        {
            // Arrange
            Project project = new ProjectMock(@"c:\somedirthatdoesntexist");

            // Act
            PublishProfile publishProfile = PublishProfile.Load(project, @"c:\foo-bar.cf.pubxml");

            // Assert
            Assert.AreEqual(string.Empty, publishProfile.Application.BuildpackUrl);
            Assert.AreEqual(null, publishProfile.Application.Command);
            Assert.AreEqual(null, publishProfile.Application.DiskQuota);
            Assert.AreEqual(0, publishProfile.Application.Domains.Count);
            Assert.AreEqual(0, publishProfile.Application.EnvironmentVariables.Count);
            Assert.AreEqual(null, publishProfile.Application.HealthCheckTimeout);
            Assert.AreEqual(project.Name.ToLowerInvariant(), publishProfile.Application.Hosts[0]);
            Assert.AreEqual(1, publishProfile.Application.InstanceCount);
            Assert.AreEqual(256, publishProfile.Application.Memory);
            Assert.AreEqual(project.Name, publishProfile.Application.Name);
            Assert.AreEqual(false, publishProfile.Application.NoHostName);
            Assert.AreEqual(false, publishProfile.Application.NoRoute);
            Assert.AreEqual(null, publishProfile.Application.Path);
            Assert.AreEqual(0, publishProfile.Application.Services.Count);
            Assert.AreEqual(string.Empty, publishProfile.Application.StackName);
            Assert.AreEqual(false, publishProfile.Application.UseRandomHostName);
        }

        [TestMethod()]
        public void SaveNonExistingProfileAndManifest()
        {
            // Arrange
            string testProjectDir = Path.Combine(Path.GetTempPath(), string.Format("savetest-{0}", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(testProjectDir);
            Project project = new ProjectMock(testProjectDir);
            string publishProfilePath = Path.Combine(testProjectDir, "Properties", "PublishProfiles", "mypush.cf.pubxml");
            string manifestPath = Path.Combine(testProjectDir, "manifest.yml");

            PublishProfile publishProfile = PublishProfile.Load(project, publishProfilePath);
            publishProfile.Organization = "doriath";
            publishProfile.Space = "menegroth";
            publishProfile.User = "beren";
            publishProfile.Password = "luthien";
            publishProfile.RefreshToken = "012345";
            publishProfile.SavedPassword = true;
            publishProfile.ServerUri = new Uri("https://my.server.url");
            publishProfile.SkipSSLValidation = false;

            publishProfile.Application.BuildpackUrl = "https://my.buildpack.url";
            publishProfile.Application.Command = "ls";
            publishProfile.Application.DiskQuota = 1234;
            publishProfile.Application.Domains.Add("domain.com");
            publishProfile.Application.EnvironmentVariables.Add("my", "var");
            publishProfile.Application.HealthCheckTimeout = 54321;
            publishProfile.Application.Hosts.AddRange(new string[] { "one", "two" });
            publishProfile.Application.InstanceCount = 121;
            publishProfile.Application.Memory = 42;
            publishProfile.Application.Name = "myapp";
            publishProfile.Application.NoHostName = true;
            publishProfile.Application.NoRoute = true;
            publishProfile.Application.Path = "/app/path";
            publishProfile.Application.Services.AddRange(new string[] { "s1", "s2" });
            publishProfile.Application.StackName = "leo";
            publishProfile.Application.UseRandomHostName = true;

            // Act
            publishProfile.Save();

            // Assert
            Assert.IsTrue(File.Exists(publishProfilePath));
            Assert.IsTrue(File.Exists(manifestPath));

            var loadedProfile = PublishProfile.Load(project, publishProfilePath);
            Assert.AreEqual(publishProfile.Manifest, loadedProfile.Manifest);
            Assert.AreEqual(publishProfile.Organization, loadedProfile.Organization);
            Assert.AreEqual(publishProfile.Password, loadedProfile.Password);
            Assert.AreEqual(publishProfile.RefreshToken, loadedProfile.RefreshToken);
            Assert.AreEqual(publishProfile.SavedPassword, loadedProfile.SavedPassword);
            Assert.AreEqual(publishProfile.ServerUri, loadedProfile.ServerUri);
            Assert.AreEqual(publishProfile.SkipSSLValidation, loadedProfile.SkipSSLValidation);
            Assert.AreEqual(publishProfile.Space, loadedProfile.Space);
            Assert.AreEqual(publishProfile.User, loadedProfile.User);
            Assert.AreEqual(null, loadedProfile.DeployTargetFile);
            Assert.AreEqual("CloudFoundry", publishProfile.WebPublishMethod);

            Assert.AreEqual(publishProfile.Application.BuildpackUrl, loadedProfile.Application.BuildpackUrl);
            Assert.AreEqual(publishProfile.Application.Command, loadedProfile.Application.Command);
            Assert.AreEqual(publishProfile.Application.DiskQuota, loadedProfile.Application.DiskQuota);
            Assert.AreEqual(publishProfile.Application. Domains[0], loadedProfile.Application.Domains[0]);
            Assert.AreEqual(publishProfile.Application.EnvironmentVariables["my"], loadedProfile.Application.EnvironmentVariables["my"]);
            Assert.AreEqual(publishProfile.Application.HealthCheckTimeout, loadedProfile.Application.HealthCheckTimeout);
            Assert.AreEqual(publishProfile.Application.Hosts[0], loadedProfile.Application.Hosts[0]);
            Assert.AreEqual(publishProfile.Application.Hosts[1], loadedProfile.Application.Hosts[1]);
            Assert.AreEqual(publishProfile.Application.InstanceCount, loadedProfile.Application.InstanceCount);
            Assert.AreEqual(publishProfile.Application.Memory, loadedProfile.Application.Memory);
            Assert.AreEqual(publishProfile.Application.Name, loadedProfile.Application.Name);
            Assert.AreEqual(publishProfile.Application.NoHostName, loadedProfile.Application.NoHostName);
            Assert.AreEqual(publishProfile.Application.NoRoute, loadedProfile.Application.NoRoute);
            Assert.AreEqual(publishProfile.Application.Path, loadedProfile.Application.Path);
            Assert.AreEqual(publishProfile.Application.Services[0], loadedProfile.Application.Services[0]);
            Assert.AreEqual(publishProfile.Application.Services[1], loadedProfile.Application.Services[1]);
            Assert.AreEqual(publishProfile.Application.StackName, loadedProfile.Application.StackName);
            Assert.AreEqual(publishProfile.Application.UseRandomHostName, loadedProfile.Application.UseRandomHostName);
        }
    }
}
