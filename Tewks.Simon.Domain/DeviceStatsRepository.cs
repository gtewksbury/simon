using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    static class DeviceStatsRepository
    {
        private static readonly StorageCredentials CREDENTIALS = new StorageCredentials("simonstorageaccount", "<ACCESS KEY>");
        private static readonly CloudBlobContainer CONTAINER = new CloudBlobContainer(new Uri("http://simonstorageaccount.blob.core.windows.net/stats/"), CREDENTIALS);

        public static async Task Save(DeviceStats stats)
        {
            var content = JsonConvert.SerializeObject(stats);
            await CONTAINER.CreateIfNotExistsAsync();
            var blob = CONTAINER.GetBlockBlobReference("stats.json");
            await blob.DeleteIfExistsAsync();
            await blob.UploadTextAsync(content);
        }

        public static async Task<DeviceStats> GetStats()
        {
            var blob = CONTAINER.GetBlockBlobReference("stats.json");

            if (await blob.ExistsAsync())
            {
                var content = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<DeviceStats>(content);
            }
            else
            {
                return new DeviceStats();
            }
        }
    }
}
