using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.VisualStudio.Model;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.CloudController.V2.Client;

namespace CloudFoundry.VisualStudio.UnitTests.Model
{
    [TestClass]
    public class RoutesTest
    {
        [TestMethod]
        public void RouteTest()
        {
            //Arrange
            using (ShimsContext.Create())
            {
            ListAllRoutesForSpaceResponse response = new ListAllRoutesForSpaceResponse() { Host = "testText" };
                RetrieveDomainDeprecatedResponse domain = new RetrieveDomainDeprecatedResponse() { Name = "testName" };
                PagedResponseCollection<ListAllAppsForRouteResponse> responseList = new PagedResponseCollection<ListAllAppsForRouteResponse>();
                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForRouteResponse>.AllInstances.ResourcesGet = FakeRespone;
                CloudFoundryClient client = new CloudFoundryClient(new Uri("http://api.test.xip.io"), new System.Threading.CancellationToken());


            
            //Act
                Route route = new Route(response, domain, responseList, client);

            //Assert
                Assert.IsTrue(route.Text == "testText");
                Assert.IsTrue(route.Domain == "testName");
                Assert.IsTrue(route.Apps == "testApp");
                Assert.IsNotNull(route.Icon);
                Assert.IsTrue(route.Actions.Count > 0);
        }
        }


        private System.Collections.Generic.List<ListAllAppsForRouteResponse> FakeRespone(PagedResponseCollection<ListAllAppsForRouteResponse> arg1)
        {
            return new System.Collections.Generic.List<ListAllAppsForRouteResponse>() { new ListAllAppsForRouteResponse() { Name = "testApp" } };
        }
    }
}
