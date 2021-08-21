using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Launcher
{
    public class NativeMethod
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(long fileSize, StringBuilder buffer, int bufferSize);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern bool PathRelativePathTo(
            [Out] StringBuilder pszPath,
            [In] string pszFrom,
            [In] FileAttributes dwAttrFrom,
            [In] string pszTo,
            [In] FileAttributes dwAttrTo
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);


        public static string StrFormatByteSize(long fileSize)
        {
            StringBuilder sb = new StringBuilder(16);
            StrFormatByteSize(fileSize, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string PathRelativePathTo(string parent, string file)
        {
            StringBuilder sb = new StringBuilder(260);
            PathRelativePathTo(sb, parent, FileAttributes.Directory, file, FileAttributes.Normal);

            return sb.ToString();
        }
    }

}
