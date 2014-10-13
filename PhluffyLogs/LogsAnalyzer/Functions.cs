using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;

namespace LogsAnalyzer
{
    public class Functions
    {
        public static async Task BackupBenchmarkLogs(
            [BlobTrigger("%BenchmarkLogsContainer%/{blobId}%BenchmarkDeviceInfoPrefix%")] Stream originalDeviceBlob,
            [Blob("%BenchmarkLogsContainer%/{blobId}%BenchmarkResultsPrefix%", FileAccess.Read)] Stream originalResultsBlob,
            [Blob("%BenchmarkBackupContainer%/{blobId}%BenchmarkDeviceInfoPrefix%", FileAccess.Write)] Stream backupDeviceBlob,
            [Blob("%BenchmarkBackupContainer%/{blobId}%BenchmarkResultsPrefix%", FileAccess.Write)] Stream backupResultsBlob,
            CancellationToken cancelToken)
        {
            await originalDeviceBlob.CopyToAsync(backupDeviceBlob, 4096, cancelToken);
            await originalResultsBlob.CopyToAsync(backupResultsBlob, 4096, cancelToken);
        }

        public static async Task ParseAndAnalyzeLogs(
            [BlobTrigger("%BenchmarkLogsContainer%/{blobId}%BenchmarkDeviceInfoPrefix%")] DeviceInformation deviceInformation,
            [Blob("%BenchmarkLogsContainer%/{blobId}%BenchmarkResultsPrefix%")] TextReader results,
            [Table("%BenchmarkResultsTable%")] IAsyncCollector<BenchmarkTableEntity> aggregateTable,
            [Table("%BenchmarksTable%")] IAsyncCollector<TableEntity> benchmarkIndex,
            CancellationToken cancelToken)
        {
            var csv = DataTable.New.Read(results, ',');

            var averageResultsPerBenchmark = csv.Rows
                .Select(row => row.As<BenchmarkResult>())
                .GroupBy(result => result.Benchmark)
                .Select(group => new BenchmarkTableEntity(deviceInformation)
                {
                    PartitionKey = group.Key,
                    Data_A = group.Average(result => result.Data_A),
                    Data_B = group.Average(result => result.Data_B),
                    Data_C = group.Average(result => result.Data_C),
                    Data_D = group.Average(result => result.Data_D),
                })
                .ToList();

            foreach (var entity in averageResultsPerBenchmark)
            {
                await aggregateTable.AddAsync(entity, cancelToken);
                await benchmarkIndex.AddAsync(new TableEntity()
                {
                    PartitionKey = "index",
                    RowKey = entity.PartitionKey,
                    ETag = "*"
                },
                cancelToken);
            }
        }

        public static void PoisonBlobErrorHandler(
            [QueueTrigger("webjobs-blobtrigger-poison")] PoisonBlobErrorDetails details,
            TextWriter log)
        {
            log.WriteLine("Failed to process log: " + details.BlobName);

            log.WriteLine("TODO: Sending e-mail to notify administrators...");
            // How to send e-mails using SendGrid:
            // https://github.com/victorhurdugaci/AzureWebJobsSamples/tree/master/SendEmailOnFailure
        }
    }
}
