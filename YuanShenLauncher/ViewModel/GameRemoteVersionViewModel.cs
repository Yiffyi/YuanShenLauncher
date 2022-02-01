using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Launcher.Model;
using Launcher.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.ViewModel
{
    public class GameRemoteVersionViewModel : ViewModelBase
    {

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

        private Model.MHYResource.Sdk sdk;
        public Model.MHYResource.Sdk Sdk
        {
            get => sdk;
            set => Set(ref sdk, value);
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

            Task.Run(async () =>
            {
                var res = await api.Resource();
                this.Diffs = res.Data.Game.Diffs;
                this.LatestGame = res.Data.Game.Latest;
                this.Sdk = res.Data.Sdk;

                Loading = false;
                FetchPkgCmd.RaiseCanExecuteChanged();
            });
        }

        public GameRemoteVersionViewModel()
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
