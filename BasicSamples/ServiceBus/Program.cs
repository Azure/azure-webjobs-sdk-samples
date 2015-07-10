using System.Configuration;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus
{
    class Program
    {
        private static string _servicesBusConnectionString;

        private static NamespaceManager _namespaceManager;

        public static void Main()
        {
            _servicesBusConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsServiceBus"].ConnectionString;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(_servicesBusConnectionString);
            CreateStartMessage();

            JobHostConfiguration config = new JobHostConfiguration();
            ServiceBusConfiguration serviceBusConfig = new ServiceBusConfiguration
            {
                ConnectionString = _servicesBusConnectionString
            };
            config.UseServiceBus(serviceBusConfig);

            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }

        private static void CreateStartMessage()
        {
            if (!_namespaceManager.QueueExists(Functions.StartQueueName))
            {
                _namespaceManager.CreateQueue(Functions.StartQueueName);
            }

            QueueClient queueClient = QueueClient.CreateFromConnectionString(_servicesBusConnectionString, Functions.StartQueueName);

            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write("Start");
                writer.Flush();
                stream.Position = 0;

                queueClient.Send(new BrokeredMessage(stream) { ContentType = "text/plain" });
            }

            queueClient.Close();
        }
    }
}
