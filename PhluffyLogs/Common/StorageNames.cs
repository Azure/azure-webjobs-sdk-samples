namespace Common
{
    public class StorageNames
    {
        public static string BenchmarkLogsContainerName
        {
            get
            {
                return ConfigurationReader.ReadAppSetting("BenchmarkLogsContainerName");
            }
        }

        public static string BenchmarkDeviceInfoBlobPrefix
        {
            get
            {
                return ConfigurationReader.ReadAppSetting("BenchmarkDeviceInfoBlobPrefix");
            }
        }

        public static string BenchmarkResultsBlobPrefix
        {
            get
            {
                return ConfigurationReader.ReadAppSetting("BenchmarkResultsBlobPrefix");
            }
        }

        public static string BenchmarkResultsTable
        {
            get
            {
                return ConfigurationReader.ReadAppSetting("BenchmarkResultsTable");
            }
        }

        public static string BenchmarksTable
        {
            get
            {
                return ConfigurationReader.ReadAppSetting("BenchmarksTable");
            }
        }
    }
}
