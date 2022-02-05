using Launcher.Model;
using System.Windows;
using System.Windows.Controls;

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

        public bool UsePreDownload
        {
            get { return (bool)GetValue(UsePreDownloadProperty); }
            set { SetValue(UsePreDownloadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegionProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register("Server", typeof(MHYGameServer), typeof(GameServerControl));

        public static readonly DependencyProperty UsePreDownloadProperty =
            DependencyProperty.Register("UsePreDownload", typeof(bool), typeof(GameServerControl));

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

        private void cbPre_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(UsePreDownloadProperty, (sender as CheckBox).IsChecked);
        }
    }
}
