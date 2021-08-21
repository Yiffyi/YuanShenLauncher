using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using Launcher.Model;
using Launcher.Service;

namespace Launcher.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 

        public MHYGameRegion[] DefaultRegions
        {
            get => MHYGameRegion.Defaults;
        }

        private MHYGameRegion region;
        public MHYGameRegion InputRegion
        {
            get => region;
            set => Set(ref region, value);
        }

        private MHYGameServer server;
        public MHYGameServer InputServer
        {
            get => server;
            set
            {
                Set(ref server, value);
                FetchPkgCmd.RaiseCanExecuteChanged();
            }
        }

        private List<Model.MHYResource.Diff> diffs;
        public List<Model.MHYResource.Diff> Diffs
        {
            get => diffs;
            set => Set(ref diffs, value);
        }

        private Model.MHYResource.Latest latestGame;
        public Model.MHYResource.Latest LatestGame
        {
            get => latestGame;
            set => Set(ref latestGame, value);
        }

        public bool loading = false;
        public bool Loading
        {
            get => loading;
            set
            {
                Set(ref loading, value);
                FetchPkgCmd.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand FetchPkgCmd { get; set; }
        public void FetchPkgList()
        {
            Loading = true;
            FetchPkgCmd.RaiseCanExecuteChanged();
            MHYApi api = new MHYApi(InputServer);

            DispatcherHelper.RunAsync(async () =>
            {
                var res = await api.Resource();
                this.Diffs = res.Data.Game.Diffs;
                this.LatestGame = res.Data.Game.Latest;

                Loading = false;
                FetchPkgCmd.RaiseCanExecuteChanged();
            });
        }

        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            ///
            DispatcherHelper.Initialize();
            FetchPkgCmd = new RelayCommand(FetchPkgList, () => !Loading && InputServer != null);
        }
    }
}