using Launcher.Model;
using Launcher.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher
{
    public class SolveDeltaVersionResult
    {
        public string GameVersion { get; set; }
        public string DecompressedPath { get; set; }
        public string SourceGameDirectory { get; set; }
        public IEnumerable<string> PkgVersions { get; set; }
        public Dictionary<string, MHYPkgVersion> LocalPkgVersionDict { get; set; }
        public IEnumerable<MHYPkgVersion> DuplicatedFiles { get; set; }
        public IEnumerable<MHYPkgVersion> DeltaFiles { get; set; }
    }

    public static class MHYGameHelper
    {
        public static async Task<SolveDeltaVersionResult> SolveDeltaVersion(string sourceGameDirectory, Model.MHYResource.Latest latestGame)
        {
            List<string> languagePacks = FindLanguagePacks(sourceGameDirectory);
            IEnumerable<MHYPkgVersion> localPkgVersion = ParsePkgVersion(Path.Combine(sourceGameDirectory, "pkg_version"));
            foreach (string fn in languagePacks)
            {
                localPkgVersion = localPkgVersion.Concat(ParsePkgVersion(Path.Combine(sourceGameDirectory, fn)));
            }

            IEnumerable<MHYPkgVersion> remotePkgVersion;
            string decompressedPath = latestGame.DecompressedPath;
            using (Stream stream = await MHYApi.DecompressedFile(decompressedPath, "pkg_version"))
            {
                remotePkgVersion = ParsePkgVersion(stream);
            }
            foreach (string fn in languagePacks)
            {
                using (Stream stream = await MHYApi.DecompressedFile(decompressedPath, fn))
                {
                    remotePkgVersion = remotePkgVersion.Concat(ParsePkgVersion(stream));
                }
            }

            SolveDeltaVersionResult result = new SolveDeltaVersionResult
            {
                GameVersion = latestGame.Version,
                DecompressedPath = decompressedPath,
                SourceGameDirectory = sourceGameDirectory,
                PkgVersions = languagePacks.Append("pkg_version"),
                LocalPkgVersionDict = localPkgVersion.ToDictionary(fv => fv.NeutralResourceName),
                DuplicatedFiles = remotePkgVersion.Intersect(localPkgVersion, new MHYPkgVersionCanLink()),
                DeltaFiles = remotePkgVersion.Except(localPkgVersion, new MHYPkgVersionCanLink())
            };

            return result;
        }

        public static void RecommendedAria2Conf(StreamWriter outputConf)
        {
            outputConf.Write(@"
file-allocation=falloc
check-integrity=true
console-log-level=warn
allow-overwrite=false
auto-file-renaming=false
");
            outputConf.Close();
        }

        public static void PkgVersionToAria2(IEnumerable<string> pkgVersions, IEnumerable<MHYPkgVersion> files, string decompressedPath, string targetDirectory, StreamWriter outputList)
        {
            foreach (string f in pkgVersions)
            {
                outputList.WriteLine(MHYApi.DecompressedFileUrl(decompressedPath, f).AbsoluteUri);
                outputList.WriteLine($"  dir={targetDirectory}");
                outputList.WriteLine($"  out={f}");
                outputList.WriteLine(@"  allow-overwrite=true");
            }
            foreach (MHYPkgVersion f in files)
            {
                outputList.WriteLine(MHYApi.DecompressedFileUrl(decompressedPath, f.RemoteName).AbsoluteUri);
                outputList.WriteLine($"  dir={targetDirectory}");
                outputList.WriteLine($"  out={f.RemoteName}");
                outputList.WriteLine($"  checksum=md5={f.MD5}");
            }
            outputList.Close();
        }

        public static void DownloadSdk(string url, string targetDirectory, Action<int> reportProgress)
        {
            using (var zip = new ZipArchive(new SeekableHTTPStream(url), ZipArchiveMode.Read, leaveOpen: false))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(targetDirectory);
                string text = directoryInfo.FullName;
                int total = zip.Entries.Count;
                int current = 0;
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(text, entry.FullName));
                    if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("IO_ExtractingResultsInOutside");
                    }

                    if (Path.GetFileName(fullPath).Length == 0)
                    {
                        if (entry.Length != 0L)
                        {
                            throw new IOException("IO_DirectoryNameWithData");
                        }

                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        entry.ExtractToFile(fullPath, overwrite: true);
                    }

                    reportProgress((++current) * 100 / total);
                }
            }
        }

        public static void WriteIni(SolveDeltaVersionResult result, MHYGameServer server, string targetGameDirectory)
        {
            File.WriteAllText(
                Path.Combine(targetGameDirectory, "config.ini"),
                server.ToIniConfig(result.GameVersion, ""),
                Encoding.ASCII
            );
        }
        public static void WriteIni(SolveDeltaVersionResult result, MHYGameServer server, string sdkVersion, string targetGameDirectory)
        {
            File.WriteAllText(
                Path.Combine(targetGameDirectory, "config.ini"),
                server.ToIniConfig(result.GameVersion, sdkVersion),
                Encoding.ASCII
            );
        }

        public static List<MHYPkgVersion> LinkDeltaVersion(SolveDeltaVersionResult result, string targetGameDirectory, Action<int> reportProgress)
        {
            List<MHYPkgVersion> hardLinkErrors = new List<MHYPkgVersion>();
            int total = result.DuplicatedFiles.Count();
            int cur = 0;
            foreach (MHYPkgVersion f in result.DuplicatedFiles)
            {
                Console.WriteLine($"os & cn: {f.RemoteName}");
                string fPath = Path.Combine(targetGameDirectory, f.RemoteName);
                if (!File.Exists(fPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fPath));
                    bool ret = NativeMethod.CreateHardLink(fPath, Path.Combine(result.SourceGameDirectory, result.LocalPkgVersionDict[f.NeutralResourceName].RemoteName), IntPtr.Zero);
                    if (!ret)
                    {
                        hardLinkErrors.Add(f);
                    }
                }

                reportProgress((++cur) * 100 / total);
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
