using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.VisualStudio.MSBuild;
using Microsoft.QualityTools.Testing.Fakes;
//using Microsoft.Build.Utilities;

namespace CloudFoundry.VisualStudio.UnitTests.MSBuild
{

    [TestClass]
    public class MSBuildLoggerTest
    {
        [TestMethod]
        public void MSBuildLoggerParameters()
        {
            //Arrange
            string param = "abc";
 //           CloudFoundry.VisualStudio.MSBuild.MSBuildLogger msBuildLoggerParam = new CloudFoundry.VisualStudio.MSBuild.MSBuildLogger(null,null);
            //Act
 //           msBuildLoggerParam.Parameters = param;

            //Assert
//            Assert.IsNotNull(msBuildLoggerParam.Parameters);

            using (ShimsContext.Create())
            {
                CloudController.V3.Client.Fakes.ShimCloudFoundryClient.AllInstances.AppRoutesGet = FakeRoutesGet;
                
            }
        }

        private CloudController.V3.Client.AppRoutesEndpoint FakeRoutesGet(CloudController.V3.Client.CloudFoundryClient arg1)
        {
            return null;
            //  throw new NotImplementedException();
        }
    }
}
