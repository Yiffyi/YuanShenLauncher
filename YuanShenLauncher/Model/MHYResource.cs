using Newtonsoft.Json;
using System.Collections.Generic;

namespace Launcher.Model
{

    namespace MHYResource
    {
        public class VoicePack
        {
            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }
        }

        public class Latest
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }

            [JsonProperty("entry")]
            public string Entry { get; set; }

            [JsonProperty("voice_packs")]
            public List<VoicePack> VoicePacks { get; set; }

            [JsonProperty("decompressed_path")]
            public string DecompressedPath { get; set; }
        }

        public class Diff
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }

            [JsonProperty("is_recommended_update")]
            public bool IsRecommendedUpdate { get; set; }

            [JsonProperty("voice_packs")]
            public List<VoicePack> VoicePacks { get; set; }
        }

        public class Game
        {
            [JsonProperty("latest")]
            public Latest Latest { get; set; }

            [JsonProperty("diffs")]
            public List<Diff> Diffs { get; set; }
        }

        public class PluginEntry
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }

            [JsonProperty("entry")]
            public string Entry { get; set; }
        }

        public class Plugin
        {
            [JsonProperty("plugins")]
            public List<PluginEntry> Plugins { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }
        }

        public class DeprecatedPackage
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }
        }

        public class Data
        {
            [JsonProperty("game")]
            public Game Game { get; set; }

            [JsonProperty("plugin")]
            public Plugin Plugin { get; set; }

            [JsonProperty("web_url")]
            public string WebUrl { get; set; }

            [JsonProperty("force_update")]
            public object ForceUpdate { get; set; }

            [JsonProperty("pre_download_game")]
            public Game PreDownloadGame { get; set; }

            [JsonProperty("deprecated_packages")]
            public List<DeprecatedPackage> DeprecatedPackages { get; set; }

            [JsonProperty("sdk")]
            public object Sdk { get; set; }
        }

        public class Root
        {
            [JsonProperty("retcode")]
            public int Retcode { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }
    }
}
