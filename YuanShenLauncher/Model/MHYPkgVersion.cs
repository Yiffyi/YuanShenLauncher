using System;
using System.Collections.Generic;

namespace Launcher.Model
{

    public class MHYPkgVersion
    {
        public string RemoteName { get; set; }

        // 忽略 海外 与 国服 的名称差异
        private readonly Lazy<string> neutralResourceName;
        public string NeutralResourceName
        {
            get => neutralResourceName.Value;
        }

        public string MD5 { get; set; }
        public long FileSize { get; set; }
        public bool IsPatch { get; set; }

        MHYPkgVersion()
        {
            neutralResourceName = new Lazy<string>(() =>
            {
                // len("YuanShen_Data\") = 14
                if (RemoteName.StartsWith(@"YuanShen_Data/"))
                {
                    return RemoteName.Substring(14);
                }

                // len("GenshinImpact_Data\") = 19
                if (RemoteName.StartsWith(@"GenshinImpact_Data/"))
                {
                    return RemoteName.Substring(19);
                }

                return RemoteName;
            });
        }
    }

    public class MHYPkgVersionCanLink : EqualityComparer<MHYPkgVersion>
    {
        public override bool Equals(MHYPkgVersion x, MHYPkgVersion y)
        {
            return x.NeutralResourceName == y.NeutralResourceName && x.MD5 == y.MD5;
        }

        public override int GetHashCode(MHYPkgVersion obj)
        {
            return obj.NeutralResourceName.GetHashCode() ^ obj.MD5.GetHashCode();
        }
    }
}
