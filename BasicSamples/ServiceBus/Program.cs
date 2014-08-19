using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Microsoft.Azure.Jobs;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus
{
    class Program
    {
        private const string PrefixForAll = "s-";

        private const string QueueNamePrefix = PrefixForAll + "queue-";
        private const string StartQueueName = QueueNamePrefix + "start";

        private const string TopicName = PrefixForAll + "topic";

        private static string _servicesBusConnectionString;

        private static NamespaceManager _namespaceManager;

        // Passes  service bus message from a queue to another queue using strings
        public static void SBQueue2SBQueue(
            [ServiceBusTrigger(StartQueueName)] string start,
            [ServiceBus(QueueNamePrefix + "1")] out string message,
            TextWriter log)
        {
            message = start + "-SBQueue2SBQueue";
            log.WriteLine("SBQueue2SBQueue: " + message);
        }

        // Passes a service bus message from a queue to topic using a brokered message
        public static void SBQueue2SBTopic(
            [ServiceBusTrigger(QueueNamePrefix + "1")] string message,
            [ServiceBus(TopicName)] out BrokeredMessage output,
            TextWriter log)
        {
            message = message + "-SBQueue2SBTopic";

            Stream stream = new MemoryStream();
            TextWriter writer = new StreamWriter(stream);
            writer.Write(message);
            writer.Flush();
            stream.Position = 0;

            output = new BrokeredMessage(stream);

            log.WriteLine("SBQueue2SBTopic: " + message);
        }

        // Topic subscription listener #1
        public static void SBTopicListener1(
            [ServiceBusTrigger(TopicName, QueueNamePrefix + "topic-1")] string message,
            TextWriter log)
        {
            log.WriteLine("SBTopicListener1: " + message);
        }

        // Topic subscription listener #2
        public static void SBTopicListener2(
            [ServiceBusTrigger(TopicName, QueueNamePrefix + "topic-2")] BrokeredMessage message,
            TextWriter log)
        {
            using (Stream stream = message.GetBody<Stream>())
            using (TextReader reader = new StreamReader(stream))
            {
                log.WriteLine("SBTopicListener2" + reader.ReadToEnd());
            }
        }

        public static void Main()
        {
            _servicesBusConnectionString = ConfigurationManager.ConnectionStrings["ServiceBus"].ConnectionString;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(_servicesBusConnectionString);
            CreateStartMessage();

            JobHostConfiguration config = new JobHostConfiguration()
            {
                ServiceBusConnectionString = _servicesBusConnectionString,
            };

            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }

        private static void CreateStartMessage()
        {
            if (!_namespaceManager.QueueExists(StartQueueName))
            {
                _namespaceManager.CreateQueue(StartQueueName);
            }

            QueueClient queueClient = QueueClient.CreateFromConnectionString(_servicesBusConnectionString, StartQueueName);

            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write("Start");
                writer.Flush();
                stream.Position = 0;

                queueClient.Send(new BrokeredMessage(stream));
            }

            queueClient.Close();
        }
    }
}
