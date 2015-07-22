using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.VisualStudio;
using CloudFoundry.VisualStudio_UnitTests.MyToolWindowTest;

namespace CloudFoundry.VisualStudio.UnitTests
{
    [TestClass()]
    public class PackageTest
    {
        [TestMethod()]
        public void CreateInstance()
        {
            CloudFoundryVisualStudioPackage package = new CloudFoundryVisualStudioPackage();
        }

        [TestMethod()]
        public void IsIVsPackage()
        {
            CloudFoundryVisualStudioPackage package = new CloudFoundryVisualStudioPackage();
            Assert.IsNotNull(package as IVsPackage, "The object does not implement IVsPackage");
        }

        [TestMethod()]
        public void SetSite()
        {
            // Create the package
            IVsPackage package = new CloudFoundryVisualStudioPackage() as IVsPackage;
            Assert.IsNotNull(package, "The object does not implement IVsPackage");

            // Create a basic service provider
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

            // Add site support for activity log
            BaseMock activityLogMock = new GenericMockFactory("MockVsActivityLog", new[] { typeof(Microsoft.VisualStudio.Shell.Interop.IVsActivityLog) }).GetInstance();
            serviceProvider.AddService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsActivityLog), activityLogMock, true);

            // Add site support to register editor factory
            BaseMock registerEditor = RegisterEditorMock.GetRegisterEditorsInstance();
            serviceProvider.AddService(typeof(SVsRegisterEditors), registerEditor, false);

            // Site the package
            Assert.AreEqual(0, package.SetSite(serviceProvider), "SetSite did not return S_OK");

            // Unsite the package
            Assert.AreEqual(0, package.SetSite(null), "SetSite(null) did not return S_OK");
        }
    }
}
