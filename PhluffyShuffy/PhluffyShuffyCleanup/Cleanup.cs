using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using PhluffyShuffyWebData;

namespace PhluffyShuffyCleanup
{
    public class Cleanup
    {
        // The number of days a shuffle is kept in the system before we delete it
        private const int RetentionPolicyDays = 2;

        private readonly IImageStorage storage;

        public Cleanup(IImageStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
        }

        // This job will only triggered on demand
        [NoAutomaticTrigger()]
        public static void CleanupFunction(CloudStorageAccount storageAccount, TextWriter log)
        {
            Cleanup cleanupClass = new Cleanup(new AzureImageStorage(storageAccount));
            cleanupClass.DoCleanup(log);
        }

        public void DoCleanup(TextWriter log)
        {
            IEnumerable<string> oldShuffles = this.storage.GetShufflesOlderThan(DateTime.UtcNow.AddDays(-RetentionPolicyDays));
            foreach(string shuffle in oldShuffles)
            {
                log.WriteLine("Deleting old container {0}", shuffle);
                this.storage.Delete(shuffle);
            }
        }
    }
}
