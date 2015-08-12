using System;
using System.Configuration;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace QueueOperations
{
    class Program
    {
        static void Main()
        {
            CreateDemoData();

            JobHost host = new JobHost();
            host.Start();

            // Stop the host if Ctrl + C/Ctrl + Break is pressed
            Console.CancelKeyPress += (sender, args) =>
            {
                host.Stop();
            };

            while(true)
            {
                Thread.Sleep(500);
            }
        }

        private static void CreateDemoData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("initialorder");
            queue.CreateIfNotExists();

            Order person = new Order()
            {
                Name = "Alex",
                OrderId = Guid.NewGuid().ToString("N").ToLower()
            };

            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(person)));
        }
    }
}
