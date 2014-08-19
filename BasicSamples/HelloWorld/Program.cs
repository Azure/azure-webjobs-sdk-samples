using System.Configuration;
using Microsoft.Azure.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HelloWorld
{
    class Program
    {
        /// <summary>
        /// Reads a message as string for the queue named "inputtext"
        /// Outputs the text in the blob helloworld/out.txt
        /// </summary>
        public static void HelloWorldFunction([QueueTrigger("inputText")] string inputText, [Blob("helloworld/out.txt")] out string output)
        {
            output = inputText;
        }

        static void Main()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureJobsStorage"].ConnectionString);
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
