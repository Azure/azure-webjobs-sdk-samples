using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Microsoft.Azure.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace MiscOperations
{
    class Program
    {
        private static string shutDownFile = Path.GetTempFileName();

        /// <summary>
        /// This function demonstrates how cancellation tokens can be used to gracefully shutdown a webjob. 
        /// </summary>
        /// <param name="message">A queue message used to trigger the function</param>
        /// <param name="log">A text writer associated with the output log from dashboard.</param>
        /// <param name="cancelToken">The cancellation token that is signaled when the job must stop</param>
        public static void ShutdownMonitor(
            [QueueTrigger("%ShutdownQueueName%")] string message,
            TextWriter log,
            CancellationToken cancelToken)
        {
            // Simulate what happens in Azure WebSites
            // as described here http://blog.amitapple.com/post/2014/05/webjobs-graceful-shutdown/#.U3aIXRFOVaQ 
            new Thread(new ThreadStart(() =>
            {
                log.WriteLine("From thread: In about 10 seconds, the job will be signaled to stop");
                Thread.Sleep(10000);

                // Modify the shutdown file
                File.WriteAllText(shutDownFile, string.Empty);
            })).Start();


            log.WriteLine("From function: Received a message: " + message);

            while (!cancelToken.IsCancellationRequested)
            {
                log.WriteLine("From function: Cancelled: No");
                Thread.Sleep(2000);
            }

            // Perform the graceful shutdown logic here
            log.WriteLine("From function: Cancelled: Yes");
        }

        static void Main()
        {
            CreateTriggerMessage();

            JobHostConfiguration hostConfiguration = new JobHostConfiguration()
            {
                NameResolver = new ConfigNameResolver()
            };

            try
            {
                SetEnvironmentVariable(shutDownFile);
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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureJobsStorage"].ConnectionString);

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
