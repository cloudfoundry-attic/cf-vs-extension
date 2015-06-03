using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CloudFoundry.VisualStudio.UnitTests.MyToolWindowTest
{
    class WindowFrameMock
    {
        const string propertiesName = "properties";

        private static GenericMockFactory frameFactory = null;

        /// <summary>
        /// Return a IVsWindowFrame without any special implementation
        /// </summary>
        /// <returns></returns>
        internal static IVsWindowFrame GetBaseFrame()
        {
            if (frameFactory == null)
                frameFactory = new GenericMockFactory("WindowFrame", new Type[] { typeof(IVsWindowFrame), typeof(IVsWindowFrame2) });
            IVsWindowFrame frame = (IVsWindowFrame)frameFactory.GetInstance();
            return frame;
        }
    }
}
