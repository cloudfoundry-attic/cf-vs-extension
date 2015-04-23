using System;
using System.Drawing;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.Model
{
    internal enum CloudItemActionContinuation
    {
        None,
        RefreshChildren,
        RefreshParent
    }
}
