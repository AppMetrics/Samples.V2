using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Health;
using App.Metrics.Health.Builder;
using static System.Console;

namespace Net452.Health.Console.QuickStart
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var health = new HealthBuilder()
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

            var healthStatus = await health.HealthCheckRunner.ReadAsync();

            using (var stream = new MemoryStream())
            {
                await health.DefaultOutputHealthFormatter.WriteAsync(stream, healthStatus);
                var result = Encoding.UTF8.GetString(stream.ToArray());
                WriteLine(result);
            }

            ReadKey();
        }
    }
}
