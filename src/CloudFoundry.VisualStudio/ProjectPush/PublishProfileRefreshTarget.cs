using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal enum PublishProfileRefreshTarget
    {
        Client,
        Organizations,
        Spaces,
        ServiceInstances,
        Stacks,
        Buildpacks,
        SharedDomains,
        PrivateDomains
    }
}
