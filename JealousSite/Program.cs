using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace JealousSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Builder b = new Builder();
                b.RebuildAll();
            }
            catch { }
            if (args.Length == 1 && args[0] == "--build") return;
            using (var watcher = new Watcher())
            {
                var host = new WebHostBuilder()
                    .UseUrls("http://0.0.0.0:5000")
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .Build();
                host.Run();
            }
        }
    }
}
