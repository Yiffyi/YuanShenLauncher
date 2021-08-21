using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        [STAThread]
        public static void Main()
        {
            App.Main(); //启动WPF项目
        }

        //解析程序集失败，会加载对应的程序集
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            //判断程序集的区域性
            if (!assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture))
            {
                path = Path.Combine(assemblyName.CultureInfo.ToString(), path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    byte[] assemblyRawBytes = new byte[stream.Length];
                    stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                    return Assembly.Load(assemblyRawBytes);
                }

                return null;
            }
        }
    }
}
