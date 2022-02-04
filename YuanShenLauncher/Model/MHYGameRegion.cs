using System;
using System.Collections.Generic;

namespace Launcher.Model
{
    public class MHYGameRegion
    {
        public static readonly MHYGameRegion[] Defaults = new MHYGameRegion[]
        {

            new MHYGameRegion
            {
                Name = "国服（CN）",
                Tag = "CN",
                Servers = new List<MHYGameServer>(new MHYGameServer[]
                {
                    new MHYGameServer
                    {
                        Name = "天空岛（官服）",
                        LauncherId = 18,
                        ChannelId = 1,
                        SubChannelId = 1,
                        SdkStatic = new Uri("https://sdk-static.mihoyo.com/hk4e_cn/"),
                        ResourceKey = "eYd89JmJ",
                        Cps = "pcadbdpz"
                    },
                    new MHYGameServer
                    {
                        Name = "世界树（Bilibili 服）",
                        LauncherId = 17,
                        ChannelId = 14,
                        SubChannelId = 0,
                        SdkStatic = new Uri("https://sdk-static.mihoyo.com/hk4e_cn/"),
                        ResourceKey = "KAtdSsoQ",
                        Cps = "bilibili"
                    }
                })
            },
            new MHYGameRegion
            {
                Name = "海外（OS）",
                Tag = "OS",
                Servers = new List<MHYGameServer>(new MHYGameServer[]
                {
                    new MHYGameServer
                    {
                        Name = "国际服",
                        LauncherId = 10,
                        ChannelId = 1,
                        SubChannelId = 0,
                        SdkStatic = new Uri("https://sdk-os-static.mihoyo.com/hk4e_global/"),
                        ResourceKey = "gcStgarh",
                        Cps = "mihoyo"
                    }
                })
            }
        };

        public string Name { get; set; }
        public string Tag { get; set; }
        public List<MHYGameServer> Servers { get; set; }
    }
}
