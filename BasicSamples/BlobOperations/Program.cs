using System;
using System.Configuration;
using System.IO;
using Microsoft.Azure.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace BlobOperations
{
    class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    class Program
    {
        /// <summary>
        /// Reads a blob from the container named "input" and writes it to the container named "output". The blob name ("name") is preserved
        /// </summary>
        public static void BlobToBlob([BlobTrigger("input/{name}")] TextReader input, [Blob("output/{name}")] out string output)
        {
            output = input.ReadToEnd();
        }

        /// <summary>
        /// This function is triggered when a new blob is created by "BlobToBlob"
        /// </summary>
        public static void BlobTrigger([BlobTrigger("output/{name}")] Stream input, TextWriter log)
        {
            using (StreamReader reader = new StreamReader(input))
            {
                log.WriteLine("Blob content: {0}", reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Reads a "Person" object from the "persons" queue
        /// The parameter "Name" will have the same value as the property "Name" of the person object
        /// The output blob will have the name of the "Name" property of the person object
        /// </summary>
        public static void BlobNameFromQueueMessage([QueueTrigger("persons")] Person persons, string Name, [Blob("persons/{Name}BlobNameFromQueueMessage")] out string output)
        {
            output = "Hello " + Name;
        }

        /// <summary>
        /// Same as "BlobNameFromQueueMessage" but using IBinder 
        /// </summary>
        public static void BlobIBinder([QueueTrigger("persons")] Person persons, IBinder binder)
        {
            TextWriter writer = binder.Bind<TextWriter>(new BlobAttribute("persons/" + persons.Name + "BlobIBinder"));
            writer.Write("Hello " + persons.Name);
        }

        /// <summary>
        /// Not writing anything into the output stream will not lead to blob creation
        /// </summary>
        public static void BlobCancelWrite([QueueTrigger("persons")] Person persons, [Blob("output/ShouldNotBeCreated.txt")] TextWriter output)
        {
            // Do not write anything to "output" and the blob will not be created
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
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("input");
            container.CreateIfNotExists();

            CloudBlockBlob blob = container.GetBlockBlobReference("BlobOperations");
            blob.UploadText("Hello world!");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("persons");
            queue.CreateIfNotExists();

            Person person = new Person()
            {
                Name = "John",
                Age = 42
            };

            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(person)));
        }
    }
}
