using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("textinput");
            queue.CreateIfNotExists();

            queue.AddMessage(new CloudQueueMessage("Hello world!"));
        }
    }
}
