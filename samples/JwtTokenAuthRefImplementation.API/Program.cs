using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace JwtTokenAuthRefImplementation.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(cfg => cfg.UseStartup<Startup>());
    }
}