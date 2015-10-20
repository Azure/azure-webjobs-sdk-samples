using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace TableOperations
{
    class Program
    {

        static void Main()
        {
            CreateDemoData();

            JobHost host = new JobHost();
            Task callTask = host.CallAsync(typeof(Functions).GetMethod("ManualTrigger"));
            
            Console.WriteLine("Waiting for async operation...");
            callTask.Wait();
            Console.WriteLine("Task completed: " + callTask.Status);

            host.RunAndBlock();
        }

        private static void CreateDemoData()
        {
            string connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("textinput");
            queue.CreateIfNotExists();

            queue.AddMessage(new CloudQueueMessage("Hello world!"));
        }
    }
}
