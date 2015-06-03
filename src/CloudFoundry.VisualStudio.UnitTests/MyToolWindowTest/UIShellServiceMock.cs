using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VsSDK.UnitTestLibrary;

namespace CloudFoundry.VisualStudio.UnitTests.MyToolWindowTest
{
    static class UIShellServiceMock
    {
        private static GenericMockFactory uiShellFactory;

        #region UiShell Getters
        /// <summary>
        /// Returns an IVsUiShell that does not implement any methods
        /// </summary>
        /// <returns></returns>
        internal static BaseMock GetUiShellInstance()
        {
            if (uiShellFactory == null)
            {
                uiShellFactory = new GenericMockFactory("UiShell", new Type[] { typeof(IVsUIShell), typeof(IVsUIShellOpenDocument) });
            }
            BaseMock uiShell = uiShellFactory.GetInstance();
            return uiShell;
        }

        /// <summary>
        /// Get an IVsUiShell that implement CreateToolWindow
        /// </summary>
        /// <returns>uishell mock</returns>
        internal static BaseMock GetUiShellInstanceCreateToolWin()
        {
            BaseMock uiShell = GetUiShellInstance();
            string name = string.Format("{0}.{1}", typeof(IVsUIShell).FullName, "CreateToolWindow");
            uiShell.AddMethodCallback(name, new EventHandler<CallbackArgs>(CreateToolWindowCallBack));

            return uiShell;
        }

        /// <summary>
        /// Get an IVsUiShell that implement CreateToolWindow (negative test)
        /// </summary>
        /// <returns>uishell mock</returns>
        internal static BaseMock GetUiShellInstanceCreateToolWinReturnsNull()
        {
            BaseMock uiShell = GetUiShellInstance();
            string name = string.Format("{0}.{1}", typeof(IVsUIShell).FullName, "CreateToolWindow");
            uiShell.AddMethodCallback(name, new EventHandler<CallbackArgs>(CreateToolWindowNegativeTestCallBack));

            return uiShell;
        }
        #endregion

        #region Callbacks
        private static void CreateToolWindowCallBack(object caller, CallbackArgs arguments)
        {
            arguments.ReturnValue = VSConstants.S_OK;

            // Create the output mock object for the frame
            IVsWindowFrame frame = WindowFrameMock.GetBaseFrame();
            arguments.SetParameter(9, frame);
        }

        private static void CreateToolWindowNegativeTestCallBack(object caller, CallbackArgs arguments)
        {
            arguments.ReturnValue = VSConstants.S_OK;

            //set the windowframe object to null
            arguments.SetParameter(9, null);
        }
        #endregion
    }
}