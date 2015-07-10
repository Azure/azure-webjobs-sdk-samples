using System;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MiscOperations
{
    class Program
    {
        static void Main()
        {
            CreateTriggerMessage();

            JobHostConfiguration hostConfiguration = new JobHostConfiguration()
            {
                NameResolver = new ConfigNameResolver()
            };

            try
            {
                SetEnvironmentVariable(Functions.ShutDownFilePath);
                JobHost host = new JobHost(hostConfiguration);
                host.RunAndBlock();
            }
            finally
            {
                ClearEnvironmentVariable();
            }

            Console.WriteLine("\nDone");
            Console.ReadLine();
        }

        private static void CreateTriggerMessage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("testqueue");
            queue.CreateIfNotExists();

            queue.AddMessage(new CloudQueueMessage("GO!"));
        }

        private static void SetEnvironmentVariable(string path)
        {
            Environment.SetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE", path);
        }

        private static void ClearEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE", null);
        }
    }
}
