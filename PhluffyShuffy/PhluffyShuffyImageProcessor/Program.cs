using Microsoft.Azure.WebJobs;

namespace PhluffyShuffyImageProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            JobHost jobHost = new JobHost();
            jobHost.RunAndBlock();
        }
    }
}
