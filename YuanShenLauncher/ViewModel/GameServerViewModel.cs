using GalaSoft.MvvmLight;
using Launcher.Model;

namespace Launcher.ViewModel
{
    public class GameServerViewModel : ViewModelBase
    {
        public MHYGameRegion[] DefaultRegions
        {
            get => MHYGameRegion.Defaults;
        }
    }
}
