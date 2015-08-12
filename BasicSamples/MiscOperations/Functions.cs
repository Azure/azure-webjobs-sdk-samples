using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace MiscOperations
{
    // { "ID": 1, "Region": "West", "Zone": 2, "Category": 3, "Description": "Fix It!" }
    public class WorkItem
    {
        public int ID { get; set; }
        public string Region { get; set; }
        public int Zone { get; set; }
        public int Category { get; set; }
        public string Description { get; set; }
    }

    public class Functions
    {
        public const string ServiceBusTestQueueName = "sb-queue-test";
        private static string shutDownFile = Path.GetTempFileName();

        public static string ShutDownFilePath
        {
            get
            {
                return shutDownFile;
            }
        }

        public static void ServiceBusJob(
            [ServiceBusTrigger(ServiceBusTestQueueName)] string message,
            TextWriter log)
        {
            log.WriteLine("ServiceBus message received!: " + message);
        }

        /// <summary>
        /// This function demonstrates how cancellation tokens can be used to gracefully shutdown a webjob. 
        /// </summary>
        /// <param name="message">A queue message used to trigger the function</param>
        /// <param name="log">A text writer associated with the output log from dashboard.</param>
        /// <param name="cancellationToken">The cancellation token that is signaled when the job must stop</param>
        public static void ShutdownMonitorJob(
            [QueueTrigger("%ShutdownQueueName%")] string message,
            TextWriter log,
            CancellationToken cancellationToken)
        {
            // Simulate what happens in Azure WebSites
            // as described here http://blog.amitapple.com/post/2014/05/webjobs-graceful-shutdown/#.U3aIXRFOVaQ 
            new Thread(new ThreadStart(() =>
            {
                log.WriteLine("From thread: In about 10 seconds, the job will be signaled to stop");
                Thread.Sleep(10000);

                // Modify the shutdown file
                File.WriteAllText(shutDownFile, string.Empty);
            })).Start();

            log.WriteLine("From function: Received a message: " + message);

            while (!cancellationToken.IsCancellationRequested)
            {
                log.WriteLine("From function: Cancelled: No");
                Thread.Sleep(2000);
            }

            // Perform the graceful shutdown logic here
            log.WriteLine("From function: Cancelled: Yes");
        }

        /// <summary>
        /// Shows use of Singleton to serialize function invocations across all host instances (scale out).
        /// Only a single instance of this function will be invoked at a time for the specified scope.
        /// 
        /// To test this, queue up 3 or more WorkItem messages before starting the host. You'll see
        /// that they are serialized. Comment out the Singleton attribute, and they'll be executed
        /// concurrently.
        /// To witness lock renewal for long running functions, set the below delay to a minute
        /// and in the console you should see periodic renewals showing the lock is maintained.
        /// </summary>
        [Singleton(@"{Region}\{Zone}")]
        public static async Task SingletonJob([QueueTrigger("singleton-test")] WorkItem workItem, TraceWriter trace)
        {
            trace.Info("Singleton method started");

            await Task.Delay(10 * 1000);

            trace.Info("Singleton method completed");
        }
    }
}
