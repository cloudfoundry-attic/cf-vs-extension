using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class CFProfile
    {
        private PublishProfile publishProfile;

        internal CFProfile(PublishProfile publishProfile)
        {
            this.publishProfile = publishProfile;
        }
    }
}
