using Launcher.Model;
using Launcher.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher
{
    public class SolveDeltaVersionResult
    {
        public MHYApi RemoteApi { get; set; }
        public string DecompressedPath { get; set; }
        public string SourceGameDirectory { get; set; }
        public IEnumerable<string> PkgVersions { get; set; }
        public Dictionary<string, MHYPkgVersion> LocalPkgVersionDict { get; set; }
        public IEnumerable<MHYPkgVersion> DuplicatedFiles { get; set; }
        public IEnumerable<MHYPkgVersion> DeltaFiles { get; set; }
    }

    public static class MHYGameHelper
    {
        public static async Task<SolveDeltaVersionResult> SolveDeltaVersion(string sourceGameDirectory, MHYApi remoteApi)
        {
            List<string> languagePacks = FindLanguagePacks(sourceGameDirectory);
            IEnumerable<MHYPkgVersion> localPkgVersion = ParsePkgVersion(Path.Combine(sourceGameDirectory, "pkg_version"));
            foreach (string fn in languagePacks)
            {
                localPkgVersion = localPkgVersion.Concat(ParsePkgVersion(Path.Combine(sourceGameDirectory, fn)));
            }

            IEnumerable<MHYPkgVersion> remotePkgVersion;
            Model.MHYResource.Root osResources = await remoteApi.Resource();
            string decompressedPath = osResources.Data.Game.Latest.DecompressedPath;
            using (Stream stream = await remoteApi.DecompressedFile(decompressedPath, "pkg_version"))
            {
                remotePkgVersion = ParsePkgVersion(stream);
            }
            foreach (string fn in languagePacks)
            {
                using (Stream stream = await remoteApi.DecompressedFile(decompressedPath, fn))
                {
                    remotePkgVersion = remotePkgVersion.Concat(ParsePkgVersion(stream));
                }
            }

            SolveDeltaVersionResult result = new SolveDeltaVersionResult
            {
                RemoteApi = remoteApi,
                DecompressedPath = decompressedPath,
                SourceGameDirectory = sourceGameDirectory,
                PkgVersions = languagePacks.Append("pkg_version"),
                LocalPkgVersionDict = localPkgVersion.ToDictionary(fv => fv.ResourceName),
                DuplicatedFiles = remotePkgVersion.Intersect(localPkgVersion, new MHYPkgVersionCanLink()),
                DeltaFiles = remotePkgVersion.Except(localPkgVersion, new MHYPkgVersionCanLink())
            };

            return result;
        }

        public static void DeltaFilesToAria2(SolveDeltaVersionResult result, string targetGameDirectory, StreamWriter outputList)
        {
            foreach(string f in result.PkgVersions)
            {
                outputList.WriteLine(result.RemoteApi.DecompressedFileUrl(result.DecompressedPath, f).AbsoluteUri);
                outputList.WriteLine($"  dir={targetGameDirectory}");
                outputList.WriteLine($"  out={f}");
            }
            foreach (MHYPkgVersion f in result.DeltaFiles)
            {
                outputList.WriteLine(result.RemoteApi.DecompressedFileUrl(result.DecompressedPath, f.RemoteName).AbsoluteUri);
                outputList.WriteLine($"  dir={targetGameDirectory}");
                outputList.WriteLine($"  out={f.RemoteName}");
                outputList.WriteLine($"  checksum=md5={f.MD5}");
                outputList.WriteLine($"  check-integrity=true");
            }
            outputList.Close();
        }

        public static List<MHYPkgVersion> LinkDeltaVersion(SolveDeltaVersionResult result, string targetGameDirectory)
        {
            List<MHYPkgVersion> hardLinkErrors = new List<MHYPkgVersion>();
            foreach (MHYPkgVersion f in result.DuplicatedFiles)
            {
                Console.WriteLine($"os & cn: {f.RemoteName}");
                string fPath = Path.Combine(targetGameDirectory, f.RemoteName);
                if (!File.Exists(fPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fPath));
                    bool ret = NativeMethod.CreateHardLink(fPath, Path.Combine(result.SourceGameDirectory, result.LocalPkgVersionDict[f.ResourceName].RemoteName), IntPtr.Zero);
                    if (!ret)
                    {
                        hardLinkErrors.Add(f);
                    }
                }
            }
            return hardLinkErrors;
        }

        public static List<string> FindLanguagePacks(string gameDirectory)
        {
            List<string> result = new List<string>();
            foreach (string f in Directory.EnumerateFiles(gameDirectory))
            {
                string fn = Path.GetFileName(f);
                if (fn.StartsWith("Audio_") && fn.EndsWith("_pkg_version"))
                {
                    result.Add(fn);
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
