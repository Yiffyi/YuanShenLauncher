using System;
using Launcher.Service;

namespace Launcher.Model
{
    public class MHYGameServer
    {
        public string Name { get; set; }
        public int LauncherId { get; set; }
        public int ChannelId { get; set; }
        public int SubChannelId { get; set; }
        public Uri SdkStatic { get; set; }
        public string ResourceKey { get; set; }

        public MHYApi Api
        {
            get => new MHYApi(this);
        }
    }
}
