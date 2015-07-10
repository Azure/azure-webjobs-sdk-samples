using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Queues;
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

            JobHostConfiguration configuration = new JobHostConfiguration();

            // Demonstrates the global queue processing settings that can
            // be configured
            configuration.Queues.MaxPollingInterval = TimeSpan.FromSeconds(30);
            configuration.Queues.MaxDequeueCount = 10;
            configuration.Queues.BatchSize = 16;
            configuration.Queues.NewBatchThreshold = 20;

            // Demonstrates how queue processing can be customized further
            // by defining a custom QueueProcessor Factory
            configuration.Queues.QueueProcessorFactory = new CustomQueueProcessorFactory();

            JobHost host = new JobHost(configuration);
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

        /// <summary>
        /// Example implementation of a custom QueueProcessor factory that is able to customize
        /// queue processing per queue.
        /// </summary>
        private class CustomQueueProcessorFactory : IQueueProcessorFactory
        {
            public QueueProcessor Create(QueueProcessorFactoryContext context)
            {
                // demonstrates how the Queue.ServiceClient options can be configured
                context.Queue.ServiceClient.DefaultRequestOptions.ServerTimeout = TimeSpan.FromSeconds(30);

                // demonstrates how queue options can be customized
                context.Queue.EncodeMessage = true;

                if (context.Queue.Name == "initialorder")
                {
                    // demonstrates how batch processing behavior can be customized
                    // per queue (as opposed to the global settings that apply to ALL queues)
                    context.BatchSize = 30;
                    context.NewBatchThreshold = 100;

                    return new CustomQueueProcessor(context);
                }

                // return the default processor
                return new QueueProcessor(context);
            }

            /// <summary>
            /// Custom QueueProcessor demonstrating some of the virtuals that can be overridden
            /// to customize queue processing.
            /// </summary>
            private class CustomQueueProcessor : QueueProcessor
            {
                public CustomQueueProcessor(QueueProcessorFactoryContext context)
                    : base(context)
                {
                }

                public override Task<bool> BeginProcessingMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken)
                {
                    return base.BeginProcessingMessageAsync(message, cancellationToken);
                }

                public override Task CompleteProcessingMessageAsync(CloudQueueMessage message, FunctionResult result, CancellationToken cancellationToken)
                {
                    return base.CompleteProcessingMessageAsync(message, result, cancellationToken);
                }

                protected override async Task ReleaseMessageAsync(CloudQueueMessage message, FunctionResult result, TimeSpan visibilityTimeout, CancellationToken cancellationToken)
                {
                    // demonstrates how visibility timeout for failed messages can be customized
                    // the logic here could implement exponential backoff, etc.
                    visibilityTimeout = TimeSpan.FromSeconds(message.DequeueCount);

                    await base.ReleaseMessageAsync(message, result, visibilityTimeout, cancellationToken);
                }
            }
        }

        
    }
}
