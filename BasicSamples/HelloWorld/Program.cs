using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HelloWorld
{
    class Program
    {
       

        static void Main()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("inputtext");
            queue.CreateIfNotExists();
            queue.AddMessage(new CloudQueueMessage("Hello World!"));

            // The connection string is read from App.config
            JobHost host = new JobHost();
            host.RunAndBlock();
        }
    }
}
