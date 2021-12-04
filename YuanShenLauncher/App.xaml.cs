using GalaSoft.MvvmLight.Threading;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
