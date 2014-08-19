using System;
using System.Configuration;
using Microsoft.Azure.WebJobs;

namespace MiscOperations
{
    /// <summary>
    /// Can be used to resolve % % tokens from the config file
    /// </summary>
    internal class ConfigNameResolver : INameResolver
    {
        public string Resolve(string name)
        {
            string resolvedName = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrWhiteSpace(resolvedName))
            {
                throw new InvalidOperationException("Cannot resolve " + name);
            }

            return resolvedName;
        }
    }
}
