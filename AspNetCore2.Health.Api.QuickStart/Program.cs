using System;
using System.Threading.Tasks;
using App.Metrics.AspNetCore.Health;
using App.Metrics.Health;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCore2.Health.Api.QuickStart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if INLINE_CHECKS
            .ConfigureHealthWithDefaults(
                builder =>
                {
                    const int threshold = 100;
                    builder.HealthChecks.AddCheck("DatabaseConnected", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy("Database Connection OK")));
                    builder.HealthChecks.AddProcessPrivateMemorySizeCheck("Private Memory Size", threshold);
                    builder.HealthChecks.AddProcessVirtualMemorySizeCheck("Virtual Memory Size", threshold);
                    builder.HealthChecks.AddProcessPhysicalMemoryCheck("Working Set", threshold);
                    builder.HealthChecks.AddPingCheck("google ping", "google.com", TimeSpan.FromSeconds(10));
                    builder.HealthChecks.AddHttpGetCheck("github", new Uri("https://github.com/"), TimeSpan.FromSeconds(10));
                })
#endif
#if HOSTING_OPTIONS
                .ConfigureAppHealthHostingConfiguration(options =>
                {
                    // options.AllEndpointsPort = 3333;
                    options.HealthEndpoint = "/my-health";
                    options.HealthEndpointPort = 1111;
                    options.PingEndpoint = "/my-ping";
                    options.PingEndpointPort = 2222;
                })
#endif
                .UseHealth()
                .UseStartup<Startup>()
                .Build();
    }
}
