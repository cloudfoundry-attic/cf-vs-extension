using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio
{
    internal enum CloudItemType
    {
        Target,
        Organization,
        Space,
        AppsCollection,
        ServicesCollection,
        RoutesCollection,
        App,
        Service,
        Route,
        LoadingPlaceholder,
        Error,
    }
}
