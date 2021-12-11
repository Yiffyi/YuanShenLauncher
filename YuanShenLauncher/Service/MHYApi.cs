using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Launcher.Model;

namespace Launcher.Service
{

    public class MHYApi
    {
        public Uri _apiRoot;
        private int _launcherId;
        private int _channelId;
        private int _subChannelId;
        private string _resourceKey;

        public MHYApi(MHYGameServer server) : this(server.SdkStatic, server.LauncherId, server.ChannelId, server.SubChannelId, server.ResourceKey) { }

        public MHYApi(Uri root, int launcherId, int channelId, int subChannelId, string resourceKey)
        {
            _apiRoot = root;
            _launcherId = launcherId;
            _channelId = channelId;
            _subChannelId = subChannelId;
            _resourceKey = resourceKey;
        }

        private static HttpClient _client = new HttpClient();
        public static async Task<T> GetJsonObject<T>(Uri url)
        {
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<Model.MHYResource.Root> Resource()
        {
            var url = new Uri(_apiRoot, $"mdk/launcher/api/resource?channel_id={_channelId}&key={_resourceKey}&launcher_id={_launcherId}&sub_channel_id={_subChannelId}");
            return await GetJsonObject<Model.MHYResource.Root>(url);
        }

        public static Uri DecompressedFileUrl(string decompressedPath, string remoteName)
        {
            var baseUrl = new Uri(decompressedPath.Last() != '/' ? decompressedPath + '/' : decompressedPath);
            var url = new Uri(baseUrl, remoteName);

            return url;
        }

        public static async Task<Stream> DecompressedFile(string decompressedPath, string remoteName)
        {
            var url = DecompressedFileUrl(decompressedPath, remoteName);
            return await _client.GetStreamAsync(url);
        }
    }
}
