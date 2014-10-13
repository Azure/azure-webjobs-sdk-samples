using Common;
using Microsoft.WindowsAzure.Storage;

namespace LogViewer
{
    public static class StorageAccount
    {
        public static CloudStorageAccount Create()
        {
            var connectionString = ConfigurationReader.ReadConnectionString("AzureWebJobsStorage");
            return CloudStorageAccount.Parse(connectionString);
        }
    }
}