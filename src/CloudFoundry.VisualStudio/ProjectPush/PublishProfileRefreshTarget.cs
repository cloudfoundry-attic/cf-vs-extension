namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal enum PublishProfileRefreshTarget
    {
        PublishProfile,
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
