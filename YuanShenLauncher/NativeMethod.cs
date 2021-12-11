using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

        // 双引号需外部解决 https://ss64.com/nt/cmd.html
        // CMD /c ""c:\Program Files\demo1.cmd" & "c:\Program Files\demo2.cmd""
        // CMD /k ""c:\batch files\demo.cmd" "Parameter 1 with space" "Parameter2 with space""
        // cmdStr = "c:\Program Files\demo1.cmd"
        public static int RunCmd(string cmdStr, string workingDir = "", bool wait = true, bool pauseAfterFinish = false)
        {
            int ret = 0;
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                // runas 之后工作目录丢失
                p.StartInfo.Arguments = pauseAfterFinish ? $"/C \"pushd \"{workingDir}\" & {cmdStr} & pause\"" : $"/C \"pushd \"{workingDir}\" & {cmdStr}\"";
                // runas 必须要 ShellExecute
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.Verb = "runas";
                p.Start();
                if (wait) p.WaitForExit();
                ret = p.ExitCode;
            }
            return ret;
        }

        public static void ExtractAria2(string targetDirectory)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Launcher.Resource.aria2c.exe"))
            {
                using (var fs = new FileStream(Path.Combine(targetDirectory, "aria2c.exe"), FileMode.Create)) stream.CopyTo(fs);
            }
        }
    }

}
