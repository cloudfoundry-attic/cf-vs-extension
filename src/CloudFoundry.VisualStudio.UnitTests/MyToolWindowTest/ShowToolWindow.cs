/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using CloudFoundry.VisualStudio;

namespace CloudFoundry.VisualStudio_UnitTests.MyToolWindowTest
{
    [TestClass()]
    public class ShowToolWindowTest
    {

        [TestMethod()]
        public void ValidateToolWindowShown()
        {
            IVsPackage package = new CloudFoundry_VisualStudioPackage() as IVsPackage;

            // Create a basic service provider
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

            //Add uishell service that knows how to create a toolwindow
            BaseMock uiShellService = UIShellServiceMock.GetUiShellInstanceCreateToolWin();
            serviceProvider.AddService(typeof(SVsUIShell), uiShellService, false);

            // Add site support for activity log
            BaseMock activityLogMock = new GenericMockFactory("MockVsActivityLog", new[] { typeof(Microsoft.VisualStudio.Shell.Interop.IVsActivityLog) }).GetInstance();
            serviceProvider.AddService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsActivityLog), activityLogMock, true);

            // Add site support to register editor factory
            BaseMock registerEditor = RegisterEditorMock.GetRegisterEditorsInstance();
            serviceProvider.AddService(typeof(SVsRegisterEditors), registerEditor, false);

            // Site the package
            Assert.AreEqual(0, package.SetSite(serviceProvider), "SetSite did not return S_OK");

            MethodInfo method = typeof(CloudFoundry_VisualStudioPackage).GetMethod("ShowToolWindow", BindingFlags.NonPublic | BindingFlags.Instance);

            object result = method.Invoke(package, new object[] { null, null });
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException), "Did not throw expected exception when windowframe object was null")]
        public void ShowToolwindowNegativeTest()
        {
            IVsPackage package = new CloudFoundry_VisualStudioPackage() as IVsPackage;

            // Create a basic service provider
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

            //Add uishell service that knows how to create a toolwindow
            BaseMock uiShellService = UIShellServiceMock.GetUiShellInstanceCreateToolWinReturnsNull();
            serviceProvider.AddService(typeof(SVsUIShell), uiShellService, false);

            // Site the package
            Assert.AreEqual(0, package.SetSite(serviceProvider), "SetSite did not return S_OK");

            MethodInfo method = typeof(CloudFoundry_VisualStudioPackage).GetMethod("ShowToolWindow", BindingFlags.NonPublic | BindingFlags.Instance);

            //Invoke thows TargetInvocationException, but we want it's inner Exception thrown by ShowToolWindow, InvalidOperationException.
            try
            {
                object result = method.Invoke(package, new object[] { null, null });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }
    }
}
