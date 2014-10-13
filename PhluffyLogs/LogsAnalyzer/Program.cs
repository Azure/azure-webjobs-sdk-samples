using Microsoft.Azure.WebJobs;

namespace LogsAnalyzer
{
    class Program
    {
        static void Main()
        {
            var configuration = new JobHostConfiguration()
            {
                NameResolver = new ConfigurationNameResolver(),
            };

            var host = new JobHost(configuration);
            host.RunAndBlock();
        }
    }
}
