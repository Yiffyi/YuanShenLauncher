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
using Launcher.Model;

namespace Launcher.View
{
    /// <summary>
    /// GameServerControl.xaml 的交互逻辑
    /// </summary>
    public partial class GameServerControl : UserControl
    {
        public MHYGameServer Server
        {
            get { return (MHYGameServer)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegionProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register("Server", typeof(MHYGameServer), typeof(GameServerControl));


        public GameServerControl()
        {
            InitializeComponent();
            cbRegion.ItemsSource = MHYGameRegion.Defaults;
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbServer.ItemsSource = (cbRegion.SelectedItem as MHYGameRegion).Servers;
        }

        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCurrentValue(ServerProperty, cbServer.SelectedItem);
        }
    }
}
