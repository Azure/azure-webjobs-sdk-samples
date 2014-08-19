using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace PhluffyShuffyCleanup
{
    class Program
    {
        public static void Main()
        {
            JobHost jobHost = new JobHost();
            jobHost.Call(typeof(Cleanup).GetMethod("CleanupFunction"));
        }
    }
}
