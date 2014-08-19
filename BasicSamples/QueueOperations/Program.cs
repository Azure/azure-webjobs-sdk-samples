using System;
using System.Configuration;
using System.IO;
using Microsoft.Azure.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace QueueOperations
{
    class Order
    {
        public string Name { get; set; }

        public string OrderId { get; set; }
    }

    class Program
    {
        /// <summary>
        /// Reads an Order object from the "initialorder" queue
        /// Creates a blob for the specified order which contains the order details
        /// The message in "orders" will be picked up by "QueueToBlob"
        /// </summary>
        public static void MultipleOutput([QueueTrigger("initialorder")] Order order, [Blob("orders/{OrderId}")] out string orderBlob, [Queue("orders")] out string orders)
        {
            orderBlob = order.OrderId;
            orders = order.OrderId;
        }

        /// <summary>
        /// Reads a message from the "orders" queue and writes a blob in the "orders" container
        /// </summary>
        public static void QueueToBlob([QueueTrigger("orders")] string orders, IBinder binder)
        {
            TextWriter writer = binder.Bind<TextWriter>(new BlobAttribute("orders/" + orders));
            writer.Write("Completed");
        }

        /// <summary>
        /// Shows binding parameters to properties of queue messages
        /// 
        /// The "Name" parameter will get the value of the "Name" property in the Order object
        /// The "DequeueCount" parameter has a special name and its value is retrieved from the actual CloudQueueMessage object
        /// </summary>
        public static void PropertyBinding([QueueTrigger("initialorder")] Order initialorder, string Name, int dequeueCount, TextWriter log)
        {
            log.WriteLine("New order from: {0}", Name);
            log.WriteLine("Message dequeued {0} times", dequeueCount);
        }

        static void Main()
        {
            CreateDemoData();

            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        private static void CreateDemoData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureJobsStorage"].ConnectionString);

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
    }
}
