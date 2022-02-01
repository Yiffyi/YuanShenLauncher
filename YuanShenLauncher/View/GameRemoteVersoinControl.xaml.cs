using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher.View
{
    /// <summary>
    /// GameRemoteVersoinControl.xaml 的交互逻辑
    /// </summary>
    public partial class GameRemoteVersoinControl : UserControl
    {
        public GameRemoteVersoinControl()
        {
            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null) (sender as Grid).Visibility = Visibility.Visible;
            else (sender as Grid).Visibility = Visibility.Collapsed;
        }
    }
}
