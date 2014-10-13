using System;
using System.Configuration;

namespace Common
{
    public static class ConfigurationReader
    {
        public static string ReadAppSetting(string settingName)
        {
            if (string.IsNullOrWhiteSpace(settingName))
            {
                throw new ArgumentNullException("settingName");
            }

            var settingValue = ConfigurationManager.AppSettings[settingName];
            if (string.IsNullOrEmpty(settingValue))
            {
                settingValue = Environment.GetEnvironmentVariable(settingName);
            }

            if (string.IsNullOrEmpty(settingValue))
            {
                throw new InvalidOperationException("Could not find the value for setting " + settingName);
            }

            return settingValue;
        }

        public static string ReadConnectionString(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                throw new ArgumentNullException("settingName");
            }

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable(connectionStringName);
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find the value for connection string " + connectionStringName);
            }

            return connectionString;
        }
    }
}
