using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MiscOperations
{
    /// <summary>
    /// Example implementation of a custom QueueProcessor factory that is able to customize
    /// queue processing per queue.
    /// </summary>
    public class CustomQueueProcessorFactory : IQueueProcessorFactory
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
