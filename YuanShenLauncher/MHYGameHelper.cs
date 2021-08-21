using Downloader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Launcher.Model;
using Launcher.Service;

namespace Launcher
{
    public class CreateDeltaVersionResult
    {
        public IEnumerable<MHYPkgVersion> DuplicatedFiles { get; set; }
        public IEnumerable<MHYPkgVersion> DeltaFiles { get; set; }
        public List<MHYPkgVersion> HardLinkErrors { get; set; }
    }

    public static class MHYGameHelper
    {
        public static async Task<CreateDeltaVersionResult> CreateDeltaVersion(string pkgName, string sourceGameDirectory, DownloadService downloader, MHYApi remoteApi, string targetGameDirectory, bool dryRun)
        {
            IEnumerable<MHYPkgVersion> localPkgVersion = ParsePkgVersion(Path.Combine(sourceGameDirectory, pkgName));

            Model.MHYResource.Root osResources = await remoteApi.Resource();
            string decompressedPath = osResources.Data.Game.Latest.DecompressedPath;
            IEnumerable<MHYPkgVersion> remotePkgVersion;
            if (!dryRun)
            {
                await downloader.DownloadFileTaskAsync(remoteApi.DecompressedFileUrl(decompressedPath, pkgName).AbsoluteUri, Path.Combine(targetGameDirectory, pkgName));
                remotePkgVersion = ParsePkgVersion(Path.Combine(targetGameDirectory, pkgName));
            }
            else
            {
                using (var stream = await remoteApi.DecompressedFile(decompressedPath, pkgName))
                {
                    remotePkgVersion = ParsePkgVersion(stream);
                }
            }

            CreateDeltaVersionResult result = new CreateDeltaVersionResult
            {
                DuplicatedFiles = remotePkgVersion.Intersect(localPkgVersion, new MHYPkgVersionCanLink()),
                DeltaFiles = remotePkgVersion.Except(localPkgVersion, new MHYPkgVersionCanLink())
            };

            if (!dryRun)
            {

                foreach (MHYPkgVersion f in result.DeltaFiles)
                {
                    string fPath = Path.Combine(targetGameDirectory, f.RemoteName);
                    Console.WriteLine($"os - cn: {f.RemoteName}");
                    if (!File.Exists(fPath))
                    {
                        await downloader.DownloadFileTaskAsync(remoteApi.DecompressedFileUrl(decompressedPath, f.RemoteName).AbsoluteUri, fPath);
                    }
                }

                Dictionary<string, MHYPkgVersion> localPkgVersionDict = localPkgVersion.ToDictionary(fv => fv.ResourceName);
                foreach (MHYPkgVersion f in result.DuplicatedFiles)
                {
                    string fPath = Path.Combine(targetGameDirectory, f.RemoteName);
                    Console.WriteLine($"os & cn: {f.RemoteName}");
                    Directory.CreateDirectory(Path.GetDirectoryName(fPath));
                    if (!File.Exists(fPath))
                    {
                        bool ret = NativeMethod.CreateHardLink(fPath, Path.Combine(sourceGameDirectory, localPkgVersionDict[f.ResourceName].RemoteName), IntPtr.Zero);
                        if (!ret)
                        {
                            result.HardLinkErrors.Add(f);
                        }
                    }
                }
            }

            return result;
        }

        public static List<string> FindLanguagePacks(string gameDirectory)
        {
            List<string> result = new List<string>();
            foreach (var f in Directory.EnumerateFiles(gameDirectory))
            {
                string fn = Path.GetFileName(f);
                if (fn.StartsWith("Audio_") && fn.EndsWith("_pkg_version"))
                {
                    result.Append(f);
                }
            }

            return result;
        }

        public static IEnumerable<MHYPkgVersion> ParsePkgVersion(string pkgVersionFile)
        {
            return File.ReadAllLines(pkgVersionFile)
                .Select(v => JsonConvert.DeserializeObject<MHYPkgVersion>(v));
        }

        public static List<MHYPkgVersion> ParsePkgVersion(Stream stream)
        {
            List<MHYPkgVersion> result = new List<MHYPkgVersion>();
            string line;
            using (var reader = new StreamReader(stream))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    var v = JsonConvert.DeserializeObject<MHYPkgVersion>(line);
                    if (v != null) result.Add(v);
                }
            }
            return result;
        }

        // 返回错误的文件
        public static List<MHYPkgVersion> VerifyPackage(string gameDirectory, IEnumerable<MHYPkgVersion> pkgVersions, Action<int> reportProgress)
        {
            ThreadLocal<MD5> md5 = new ThreadLocal<MD5>(() => MD5.Create());
            long totalBytes = pkgVersions.Sum(item => item.FileSize);
            long bytesProcessed = 0;
            var result = pkgVersions.AsParallel().Take(5).Where(v =>
            {
                using (FileStream fs = new FileStream(Path.Combine(gameDirectory, v.RemoteName), FileMode.Open))
                {
                    byte[] h = md5.Value.ComputeHash(fs);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < h.Length; i++)
                    {
                        sb.Append(h[i].ToString("x2"));
                    }
                    long bytesLocal = Interlocked.Add(ref bytesProcessed, v.FileSize);
                    reportProgress((int)(bytesLocal * 100 / totalBytes));
                    return sb.ToString() != v.MD5.ToLower();
                }
            }).ToList();

            return result;
        }
    }
}
