using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.UnitTests
{
    public class Util
    {
        public static string PublishProfileProjectDir
        {
            get
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string projectPath = Path.Combine(assemblyPath, "project");
                return projectPath;
            }
        }

        public static string PublishProfilePath
        {
            get
            {
                string profilePath = Path.Combine(Util.PublishProfileProjectDir, "Properties", "PublishProfiles", "push.cf.pubxml");
                return profilePath;
            }
        }
    }
}
