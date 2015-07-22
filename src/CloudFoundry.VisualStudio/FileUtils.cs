namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class FileUtilities
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

        internal static string PathAddBackslash(string path)
        {
            // They're always one character but EndsWith is shorter than
            // array style access to last path character. Change this
            // if performance are a (measured) issue.
            string separator1 = Path.DirectorySeparatorChar.ToString();
            string separator2 = Path.AltDirectorySeparatorChar.ToString();

            // White spaces are always ignored (both heading and trailing)
            path = path.Trim();

            // Argument is always a directory name then if there is one
            // of allowed separators then I have nothing to do.
            if (path.EndsWith(separator1, StringComparison.OrdinalIgnoreCase) || path.EndsWith(separator2, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            // If there is the "alt" separator then I add a trailing one.
            if (path.Contains(separator2))
            {
                return path + separator2;
            }

            // If there is not an "alt" separator I add a "normal" one.
            // It means path may be with normal one or it has not any separator
            // (for example if it's just a directory name). In this case I
            // default to normal as users expect.
            return path + separator1;
        }
    }
}
