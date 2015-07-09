using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CloudFoundry.VisualStudio
{
    public class FileUtils
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrWhiteSpace(fromPath))
            {
                return toPath;
            }
            if (string.IsNullOrWhiteSpace(toPath))
            {
                return fromPath;
            }

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }
    }
}
