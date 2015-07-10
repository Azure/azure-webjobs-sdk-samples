using System.IO;
using System.Threading;
using Microsoft.Azure.WebJobs;

namespace MiscOperations
{
    public class Functions
    {
        private static string shutDownFile = Path.GetTempFileName();

        public static string ShutDownFilePath
        {
            get
            {
                return shutDownFile;
            }
        }

        /// <summary>
        /// This function demonstrates how cancellation tokens can be used to gracefully shutdown a webjob. 
        /// </summary>
        /// <param name="message">A queue message used to trigger the function</param>
        /// <param name="log">A text writer associated with the output log from dashboard.</param>
        /// <param name="cancellationToken">The cancellation token that is signaled when the job must stop</param>
        public static void ShutdownMonitor(
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
    }
}
