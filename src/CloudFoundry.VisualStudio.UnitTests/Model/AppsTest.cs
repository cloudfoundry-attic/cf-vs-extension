using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.VisualStudio.Model;

namespace CloudFoundry.VisualStudio.UnitTests.Model
{
    [TestClass]
    public class AppsTest
    {
        [TestMethod]
        public void AppTest()
        {
            //Arrange
            GetAppSummaryResponse appResponse = new GetAppSummaryResponse() {  Buildpack="testBuildpack", 
                DetectedBuildpack="testDetectBuildpack", PackageUpdatedAt="testCreation", Instances=2,
                Memory=256,Name="testAppName", RunningInstances=2};
            appResponse.Name = "testAppName";
            appResponse.State = "STARTED";

            CloudFoundryClient appClient = new CloudFoundryClient(new Uri("http://test.app.xip.io"),new System.Threading.CancellationToken());
            //Act

            App testApp = new App(appResponse, appClient);
            

            //Assert
            Assert.IsTrue(testApp.Actions.Count > 0);
            Assert.IsTrue(testApp.Buildpack == "testBuildpack");
            Assert.IsTrue(testApp.CreationDate == "testCreation");
            Assert.IsTrue(testApp.DetectedBuildpack == "testDetectBuildpack");
            Assert.IsNotNull(testApp.Icon);
            Assert.IsTrue(testApp.Instances > 0);
            Assert.IsTrue(testApp.Memory > 0);
            Assert.IsTrue(testApp.Name == "testAppName");
            Assert.IsTrue(testApp.RunningInstances > 0);
            Assert.IsTrue(testApp.Text == "testAppName");
        }
    }
}
