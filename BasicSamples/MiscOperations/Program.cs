using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MiscOperations
{
    class Program
    {
        private static string _servicesBusConnectionString;
        private static NamespaceManager _namespaceManager;
        private static CloudQueue _testQueue;

        static void Main()
        {
            CreateTestQueues();
            CreateServiceBusQueues();

            CreateServiceBusTestMessage();

            // This test message kicks off the sample on how to perform graceful
            // shutdown. It will shut down the host, so if you want to run other
            // samples, comment this out.
            //CreateShutdownTestMessage();

            JobHostConfiguration config = new JobHostConfiguration()
            {
                NameResolver = new ConfigNameResolver(),
            };

            // Demonstrates the global queue processing settings that can
            // be configured
            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(3);
            config.Queues.MaxDequeueCount = 3;
            config.Queues.BatchSize = 16;
            config.Queues.NewBatchThreshold = 20;

            // Demonstrates how queue processing can be customized further
            // by defining a custom QueueProcessor Factory
            config.Queues.QueueProcessorFactory = new CustomQueueProcessorFactory();

            // Demonstrates how the console trace level can be customized
            config.Tracing.ConsoleLevel = TraceLevel.Verbose;

            // Demonstrates how a custom TraceWriter can be plugged into the
            // host to capture all logging/traces.
            config.Tracing.Tracers.Add(new CustomTraceWriter(TraceLevel.Info));

            ServiceBusConfiguration serviceBusConfig = new ServiceBusConfiguration
            {
                ConnectionString = _servicesBusConnectionString,

                // demonstrates global customization of the default OnMessageOptions
                // that will be used by MessageReceivers
                MessageOptions = new OnMessageOptions
                {
                    MaxConcurrentCalls = 10
                }
            };

            // demonstrates use of a custom MessagingProvider to perform deeper
            // customizations of the message processing pipeline
            serviceBusConfig.MessagingProvider = new CustomMessagingProvider(serviceBusConfig);

            config.UseServiceBus(serviceBusConfig);

            try
            {
                SetEnvironmentVariable(Functions.ShutDownFilePath);
                JobHost host = new JobHost(config);
                host.RunAndBlock();
            }
            finally
            {
                ClearEnvironmentVariable();
            }

            Console.WriteLine("\nDone");
            Console.ReadLine();
        }

        private static void CreateServiceBusQueues()
        {
            _servicesBusConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);
            _namespaceManager = NamespaceManager.CreateFromConnectionString(_servicesBusConnectionString);
        }

        private static void CreateTestQueues()
        {
            string connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("singleton-test");
            queue.CreateIfNotExists();

            _testQueue = queueClient.GetQueueReference("testqueue");
            _testQueue.CreateIfNotExists();

            CloudQueue testQueue2 = queueClient.GetQueueReference("testqueue2");
            testQueue2.CreateIfNotExists();
        }

        private static void CreateShutdownTestMessage()
        {
            _testQueue.AddMessage(new CloudQueueMessage("GO!"));
        }

        private static void CreateServiceBusTestMessage()
        {
            if (!_namespaceManager.QueueExists(Functions.ServiceBusTestQueueName))
            {
                _namespaceManager.CreateQueue(Functions.ServiceBusTestQueueName);
            }

            QueueClient queueClient = QueueClient.CreateFromConnectionString(_servicesBusConnectionString, Functions.ServiceBusTestQueueName);

            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write("Test");
                writer.Flush();
                stream.Position = 0;

                queueClient.Send(new BrokeredMessage(stream) { ContentType = "text/plain" });
            }

            queueClient.Close();
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
