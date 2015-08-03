namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    internal class NativeMethods
    {
        public const int SHGFI_ICON = 0x000000100;     // get icon
        public const int SHGFI_DISPLAYNAME = 0x000000200;     // get display name
        public const int SHGFI_TYPENAME = 0x000000400;     // get type name
        public const int SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
        public const int SHGFI_ICONLOCATION = 0x000001000;     // get icon location
        public const int SHGFI_EXETYPE = 0x000002000;     // return exe type
        public const int SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
        public const int SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
        public const int SHGFI_SELECTED = 0x000010000;     // show icon in selected state
        public const int SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
        public const int SHGFI_LARGEICON = 0x000000000;     // get large icon
        public const int SHGFI_SMALLICON = 0x000000001;     // get small icon
        public const int SHGFI_OPENICON = 0x000000002;     // get open icon
        public const int SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
        public const int SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
        public const int SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}
