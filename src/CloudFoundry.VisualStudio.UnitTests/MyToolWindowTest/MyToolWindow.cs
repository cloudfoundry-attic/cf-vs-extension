using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using CloudFoundry.VisualStudio;

namespace CloudFoundry.VisualStudio.UnitTests.MyToolWindowTest
{
    /// <summary>
    ///This is a test class for MyToolWindowTest and is intended
    ///to contain all MyToolWindowTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MyToolWindowTest
    {

        /// <summary>
        ///MyToolWindow Constructor test
        ///</summary>
        [TestMethod()]
        public void MyToolWindowConstructorTest()
        {

            CloudFoundryExplorerToolWindow target = new CloudFoundryExplorerToolWindow();
            Assert.IsNotNull(target, "Failed to create an instance of MyToolWindow");

            MethodInfo method = target.GetType().GetMethod("get_Content", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method.Invoke(target, null), "MyControl object was not instantiated");

        }

        /// <summary>
        ///Verify the Content property is valid.
        ///</summary>
        [TestMethod()]
        public void WindowPropertyTest()
        {
            CloudFoundryExplorerToolWindow target = new CloudFoundryExplorerToolWindow();
            Assert.IsNotNull(target.Content, "Content property was null");
        }

    }
}
