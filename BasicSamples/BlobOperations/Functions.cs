using System.IO;
using System.Text;
using System.Threading.Tasks;
using BlobOperations;
using Microsoft.Azure.WebJobs;

namespace BlobOperations
{
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    public class Functions
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
        /// The blob name and extension will be bound from the name pattern
        /// </summary>
        public static async Task BlobTrigger(
            [BlobTrigger("output/{name}.{ext}")] Stream input, 
            string name, 
            string ext, 
            TextWriter log)
        {
            log.WriteLine("Blob name:" + name);
            log.WriteLine("Blob extension:" + ext);

            using (StreamReader reader = new StreamReader(input))
            {
                string blobContent = await reader.ReadToEndAsync();
                log.WriteLine("Blob content: {0}", blobContent);
            }
        }

        /// <summary>
        /// Reads a "Person" object from the "persons" queue
        /// The parameter "Name" will have the same value as the property "Name" of the person object
        /// The output blob will have the name of the "Name" property of the person object
        /// </summary>
        public static async Task BlobNameFromQueueMessage(
            [QueueTrigger("persons")] Person persons,
            string Name,
            [Blob("persons/{Name}BlobNameFromQueueMessage", FileAccess.Write)] Stream output)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes("Hello " + Name);

            await output.WriteAsync(messageBytes, 0, messageBytes.Length);
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
    }
}

