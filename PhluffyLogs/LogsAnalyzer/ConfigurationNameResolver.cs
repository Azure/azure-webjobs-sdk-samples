using Common;
using Microsoft.Azure.WebJobs;

namespace LogsAnalyzer
{
    internal class ConfigurationNameResolver:INameResolver
    {
        public string Resolve(string name)
        {
            return ConfigurationReader.ReadAppSetting(name);
        }
    }
}
