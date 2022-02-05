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
        [STAThread]
        public static void Main()
        {
            App.Main(); //启动WPF项目
        }
    }
}
