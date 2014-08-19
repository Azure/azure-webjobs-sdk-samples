using Microsoft.Azure.Jobs;

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
