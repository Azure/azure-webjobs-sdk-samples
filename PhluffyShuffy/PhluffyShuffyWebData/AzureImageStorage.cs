using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Newtonsoft.Json;

namespace PhluffyShuffyWebData
{
    public class AzureImageStorage : IImageStorage
    {
        private const string FinalShuffleName = "shuffle.jpg";

        private const string BlobPartPrefix = "part-";

        private const string ContainerNamePrefix = "shuffle";

        private const string ReadonlyContainerKey = "readonly";

        private const string ShuffleRequestQueueName = "shufflerequests";

        private readonly CloudStorageAccount storageAccount;

        private readonly CloudBlobClient blobClient;

        private readonly CloudQueueClient queueClient;

        public AzureImageStorage(string connectionString)
            : this(CloudStorageAccount.Parse(connectionString))
        {
        }

        public AzureImageStorage(CloudStorageAccount storageAccount)
        {
            this.storageAccount = storageAccount;
            this.blobClient = this.storageAccount.CreateCloudBlobClient();
            this.queueClient = this.storageAccount.CreateCloudQueueClient();
        }

        public IEnumerable<Uri> GetAllShuffleParts(string shuffleId)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = GetShuffleContainer(shuffleId);
            return shuffleContainer.ListBlobs(BlobPartPrefix).Select(b => b.Uri).OrderBy(u => u.ToString());
        }

        public Uri GetImageLink(string shuffleId)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = GetShuffleContainer(shuffleId);

            CloudBlockBlob shuffleBlob = shuffleContainer.GetBlockBlobReference(FinalShuffleName);
            if (!shuffleBlob.Exists())
            {
                return null;
            }

            return shuffleBlob.Uri;
        }

        public void RequestShuffle(string shuffleId)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = this.GetShuffleContainer(shuffleId);

            // Mark the container as no longer able to accept images
            shuffleContainer.Metadata[ReadonlyContainerKey] = bool.TrueString;
            shuffleContainer.SetMetadata();

            // Create the queue message and post it
            CloudQueue shuffleQueue = this.GetShuffleQueue();
            shuffleQueue.CreateIfNotExists();

            ShuffleRequestMessage newShuffleRequest = new ShuffleRequestMessage()
            {
                ShuffleId = shuffleId
            };

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            CloudQueueMessage message = new CloudQueueMessage(serializer.Serialize(newShuffleRequest));

            shuffleQueue.AddMessage(message);
        }


        public bool IsReadonly(string shuffleId)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = this.GetShuffleContainer(shuffleId);

            return shuffleContainer.Metadata[ReadonlyContainerKey] == bool.TrueString;
        }

        public void Delete(string shuffleId)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = this.GetShuffleContainer(shuffleId);

            shuffleContainer.DeleteIfExists();
        }

        public void AddNewPart(string shuffleId, string fileName, Stream fileStream)
        {
            shuffleId = ValidateAndNormalizeContainerName(shuffleId);
            CloudBlobContainer shuffleContainer = GetShuffleContainer(shuffleId);

            int blobCount = shuffleContainer.ListBlobs(BlobPartPrefix).Count();
            string blobName = BlobPartPrefix + blobCount.ToString("00") + Path.GetExtension(fileName);

            CloudBlockBlob blob = shuffleContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/jpeg";

            blob.UploadFromStream(fileStream);
        }

        private static string ValidateAndNormalizeContainerName(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException("containerName");
            }

            // Normalize the contaier name
            return containerName.ToLower();
        }

        private CloudBlobContainer GetShuffleContainer(string shuffleId)
        {
            CloudBlobContainer container = this.blobClient.GetContainerReference(ContainerNamePrefix + shuffleId);
            if (container.CreateIfNotExists())
            {
                container.Metadata.Add(ReadonlyContainerKey, bool.FalseString);
                container.SetMetadata();
                container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            }


            return container;
        }

        private CloudQueue GetShuffleQueue()
        {
            CloudQueue queue = this.queueClient.GetQueueReference(ShuffleRequestQueueName);
            queue.CreateIfNotExists();

            return queue;
        }

        public IEnumerable<string> GetShufflesOlderThan(DateTime date)
        {
            IEnumerable<CloudBlobContainer> containers = this.blobClient.ListContainers(ContainerNamePrefix);
            return containers.Where(c => c.Properties.LastModified < date).Select(c => c.Name.Replace(ContainerNamePrefix, string.Empty));
        }
    }
}
