using System;
using System.Text;
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
        public string Cps { get; set; }

        public MHYApi Api
        {
            get => new MHYApi(this);
        }

        public string ToIniConfig(string gameVersion, string sdkVersion)
        {
            var b = new StringBuilder();
            b.AppendLine("[General]");
            b.AppendLine($"channel={ChannelId}");
            b.AppendLine($"cps={Cps}");
            b.AppendLine($"game_version={gameVersion}");
            b.AppendLine($"sub_channel={SubChannelId}");
            b.AppendLine($"sdk_version={sdkVersion}");

            return b.ToString();
        }
    }
}
