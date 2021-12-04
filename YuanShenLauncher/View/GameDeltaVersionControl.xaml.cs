using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Launcher.Service;

namespace Launcher.View
{
    /// <summary>
    /// GameDeltaVersion.xaml 的交互逻辑
    /// </summary>
    public partial class GameDeltaVersionControl : UserControl
    {

        public GameDeltaVersionControl()
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

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressBar o = sender as ProgressBar;
            if (e.NewValue != 0 && e.NewValue != 100) o.Visibility = Visibility.Visible;
            else o.Visibility = Visibility.Collapsed;
        }
    }
}
