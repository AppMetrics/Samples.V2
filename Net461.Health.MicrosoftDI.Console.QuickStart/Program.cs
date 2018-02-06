using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Health;
using App.Metrics.Health.Builder;
using App.Metrics.Health.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Net461.Health.MicrosoftDI.Console.QuickStart
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            var healthBuilder = new HealthBuilder()
                .HealthChecks.RegisterFromAssembly(services, DependencyContext.Load(Assembly.GetAssembly(typeof(Program))))
                .HealthChecks.AddCheck<SampleHealthCheck>()
                .HealthChecks.AddCheck("Healthy Check",
                    () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy()))
                .HealthChecks.AddCheck("Degraded Check",
                    () => new ValueTask<HealthCheckResult>(HealthCheckResult.Degraded()))
                .HealthChecks.AddCheck("Unhealthy Check",
                    () => new ValueTask<HealthCheckResult>(HealthCheckResult.Unhealthy()))
                .HealthChecks.AddProcessPrivateMemorySizeCheck("Private Memory Size", 100)
                .HealthChecks.AddProcessVirtualMemorySizeCheck("Virtual Memory Size", 200)
                .HealthChecks.AddProcessPhysicalMemoryCheck("Working Set", 300)
                .HealthChecks.AddPingCheck("google ping", "google.com", TimeSpan.FromSeconds(10))
                .HealthChecks.AddHttpGetCheck("github", new Uri("https://github.com/"), TimeSpan.FromSeconds(10))
                .Build();

            services.AddHealth(healthBuilder);

            var provider = services.BuildServiceProvider();

            var healthCheckRunner = provider.GetRequiredService<IRunHealthChecks>();
            var healthFormatters = provider.GetRequiredService<IReadOnlyCollection<IHealthOutputFormatter>>();

            var healthStatus = await healthCheckRunner.ReadAsync();

            using (var stream = new MemoryStream())
            {
                await healthFormatters.First().WriteAsync(stream, healthStatus);
                var result = Encoding.UTF8.GetString(stream.ToArray());
                System.Console.WriteLine(result);
            }

            System.Console.ReadKey();
        }
    }
}
