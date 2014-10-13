using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace LogsAnalyzer
{
    public class DeviceInformationBlobBinder : ICloudBlobStreamBinder<DeviceInformation>
    {
        public Task<DeviceInformation> ReadFromStreamAsync(Stream input, CancellationToken cancellationToken)
        {
            using (StreamReader reader = new StreamReader(input))
            {
                var jsonString = reader.ReadToEnd();
                var deviceInfo = JsonConvert.DeserializeObject<DeviceInformation>(jsonString);
                return Task.FromResult(deviceInfo);
            }
        }

        public Task WriteToStreamAsync(DeviceInformation value, Stream output, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This method should not be needed");
        }
    }
}
