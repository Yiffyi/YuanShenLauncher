using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Launcher.Service;

namespace Launcher.View
{
    /// <summary>
    /// GameDeltaVersion.xaml 的交互逻辑
    /// </summary>
    public partial class GameDeltaVersion : UserControl
    {

        public GameDeltaVersion()
        {
            InitializeComponent();
        }

        //private void DgDamagedFiles_TargetUpdated(object sender, DataTransferEventArgs e)
        //{
        //    if (!dgDamagedFiles.HasItems)
        //    {
        //        lVerifyHint.Content = "验证文件";
        //        dgDamagedFiles.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        lVerifyHint.Content = "以下文件出现错误";
        //        dgDamagedFiles.Visibility = Visibility.Visible;
        //    }
        //}

        private void pVerify_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
