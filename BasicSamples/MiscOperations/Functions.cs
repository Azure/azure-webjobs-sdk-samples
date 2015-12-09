using System;
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

        /// <summary>
        /// Demonstrates use of <see cref="DisableAttribute"/> to allow individual jobs to be
        /// dynamically/temporarily disabled via applicaiton settings. In this example, if the
        /// app setting name "Disable_TestJob" has a value of "1" or "True" (case insensitive),
        /// the job will not run. You can see that the function is disabled in the Console output -
        /// you should see an entry "Function 'Functions.TestJob' is disabled". 
        /// 
        /// When you change these setting values in the Azure Portal, it will cause the WebJob to
        /// be restarted, picking up the new setting. This gives you a simple toggle UI allowing
        /// you to enable/disable functions as needed. 
        /// 
        /// The attribute can be declared hierarchically at the Parameter/Method/Class levels.
        /// The setting name can also contain binding parameters.
        /// </summary>
        [Disable("Disable_TestJob")]
        public static void TestJob([QueueTrigger("testqueue2")] string message)
        {
            Console.WriteLine("DisabledJob Invoked!");
        }

        /// <summary>
        /// Demonstrates use of <see cref="TimeoutAttribute"/> for signalling cancellation
        /// of job functions when a timeout expires. In the below example, the function would
        /// run for 1 day without the timeout. However, the timeout will cause the
        /// <see cref="CancellationToken"/> to be cancelled after 15 seconds.
        /// </summary>
        /// <remarks>
        /// In addition to class/method level attributes, you can also specify a global
        /// timeout via <see cref="JobHostConfiguration.FunctionTimeout"/>. Class/method
        /// level timeouts override the global timeout.
        /// </remarks>
        [Timeout("00:00:15")]
        public static async Task TimeoutJob(
            [QueueTrigger("testqueue2")] string message,
            CancellationToken token,
            TextWriter log)
        {
            await log.WriteLineAsync("Job starting");

            await Task.Delay(TimeSpan.FromDays(1), token);

            await log.WriteLineAsync("Job completed");
        }

        /// Demonstrates use of <see cref="StorageAccountAttribute"/> for operating on different
        /// Storage accounts in the same function. In this example, we're triggering on a queue
        /// in the primary account, and writing to a queue in the secondary account.
        /// <see cref="StorageAccountAttribute"/> can be applied at the class/method/parameter level.
        public static void MultipleAccounts(
            [QueueTrigger("input")] string input,
            [Queue("output"), StorageAccount("SecondaryStorage")] out string output)
        {
            output = input;
        }

        /// Demonstrates use of <see cref="ServiceBusAccountAttribute"/> for operating on different
        /// ServiceBus accounts in the same function. In this example, we're triggering on a queue
        /// in a secondary account, and writing to a topic in our primary account.
        /// <see cref="ServiceBusAccountAttribute"/> can be applied at the class/method/parameter level.
        public static void ServiceBusMultipleAccounts(
            [ServiceBusTrigger("input"), ServiceBusAccount("ServiceBusSecondary")] string input,
            [ServiceBus("output")] out string output)
        {
            output = input;
        }
    }
}
