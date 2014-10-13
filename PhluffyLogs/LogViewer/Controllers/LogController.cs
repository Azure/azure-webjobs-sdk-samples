using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace LogViewer.Controllers
{
    public class LogController : ApiController
    {
        private readonly CloudStorageAccount _storageAccount = StorageAccount.Create();

        [HttpGet]
        [Route("api/log/single/{benchmarkName}")]
        public IEnumerable<BenchmarkTableEntity> ResultsForBenchmark(string benchmarkName)
        {
            if (string.IsNullOrWhiteSpace(benchmarkName))
            {
                return new BenchmarkTableEntity[0];
            }

            var tableClient = _storageAccount.CreateCloudTableClient();
            var resultsTable = tableClient.GetTableReference(StorageNames.BenchmarkResultsTable);

            if (!resultsTable.Exists())
            {
                return new BenchmarkTableEntity[0]; 
            }

            var query = new TableQuery<BenchmarkTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, benchmarkName));

            return resultsTable.ExecuteQuery<BenchmarkTableEntity>(query);    
        }

        [HttpPost]
        [Route("api/log/benchmarkresults")]
        public async Task<HttpResponseMessage> BenchmarkResults([FromUri] DeviceInformation deviceInfo)
        {
            HttpRequestMessage request = this.Request;

            using (Stream resultsStream = await request.Content.ReadAsStreamAsync())
            {
                await StoreResults(deviceInfo, resultsStream);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task StoreResults(DeviceInformation deviceInfo, Stream results)
        {
            string logName = GenerateUniqueLogName();
            string deviceInfoBlobName = logName + StorageNames.BenchmarkDeviceInfoBlobPrefix;
            string resultsBlobName = logName + StorageNames.BenchmarkResultsBlobPrefix;

            var blobClient = _storageAccount.CreateCloudBlobClient();
            var logsContainer = blobClient.GetContainerReference(StorageNames.BenchmarkLogsContainerName);

            await logsContainer.CreateIfNotExistsAsync();

            // Upload the results first because the device blob is the one triggering the webjob
            var resultsBlob = logsContainer.GetBlockBlobReference(resultsBlobName);
            await resultsBlob.UploadFromStreamAsync(results);

            var deviceInfoBlob = logsContainer.GetBlockBlobReference(deviceInfoBlobName);
            await deviceInfoBlob.UploadTextAsync(JsonConvert.SerializeObject(deviceInfo));
        }

        private string GenerateUniqueLogName()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

}
