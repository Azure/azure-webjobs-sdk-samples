using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace MiscOperations
{
    /// <summary>
    /// Custom ServiceBus <see cref="MessagingProvider"/> demonstrating how ServiceBus
    /// message processing can be deeply customized.
    /// </summary>
    public class CustomMessagingProvider : MessagingProvider
    {
        private readonly ServiceBusConfiguration _config;

        public CustomMessagingProvider(ServiceBusConfiguration config)
            : base(config)
        {
            _config = config;
        }

        public override NamespaceManager NamespaceManager
        {
            get
            {
                // you could return your own NamespaceManager here, which would be used
                // globally
                return base.NamespaceManager;
            }
        }

        public override Task<MessagingFactory> CreateMessagingFactoryAsync(string entityPath)
        {
            // you could return a customized (or new) MessagingFactory here per entity
            return base.CreateMessagingFactoryAsync(entityPath);
        }

        public override MessageProcessor CreateMessageProcessor(string entityPath)
        {
            // demonstrates how to plug in a custom MessageProcessor
            // you could use the global MessageOptions, or use different
            // options per entity
            return new CustomMessageProcessor(_config.MessageOptions);
        }

        private class CustomMessageProcessor : MessageProcessor
        {
            public CustomMessageProcessor(OnMessageOptions messageOptions)
                : base(messageOptions)
            {
            }

            public override Task<bool> BeginProcessingMessageAsync(BrokeredMessage message, CancellationToken cancellationToken)
            {
                // intercept messages before the job function is invoked
                return base.BeginProcessingMessageAsync(message, cancellationToken);
            }

            public override Task CompleteProcessingMessageAsync(BrokeredMessage message, FunctionResult result, CancellationToken cancellationToken)
            {
                // perform any post processing after the job function has been invoked
                return base.CompleteProcessingMessageAsync(message, result, cancellationToken);
            }
        }
    }
}
