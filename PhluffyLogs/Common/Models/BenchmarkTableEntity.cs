using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Models
{
    /// <summary>
    /// PK = Benchmark; RK = Make & model
    /// </summary>
    public class BenchmarkTableEntity:TableEntity
    {
        public BenchmarkTableEntity()
        {
        }

        public BenchmarkTableEntity(DeviceInformation deviceInfo)
        {
            SetRowKey(deviceInfo);
        }

        public double Data_A { get; set; }
        public double Data_B { get; set; }
        public double Data_C { get; set; }
        public double Data_D { get; set; }

        public void SetRowKey(DeviceInformation deviceInfo)
        {
            // Use a reverse data schema to keep rows sorted chronologically 
            RowKey = string.Format("{0:D19}_{1}_{2}",
                DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks,
                deviceInfo.Manufacturer,
                deviceInfo.Model);
        }
    }
}
