using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace BlobOperations
{
    class Program
    {
        static void Main()
        {
            CreateDemoData();

            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        private static void CreateDemoData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("input");
            container.CreateIfNotExists();

            CloudBlockBlob blob = container.GetBlockBlobReference("BlobOperations.txt");
            blob.UploadText("Hello world!");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("persons");
            queue.CreateIfNotExists();

            Person person = new Person()
            {
                Name = "Mathew",
                Age = 39
            };

            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(person)));
        }
    }
}
