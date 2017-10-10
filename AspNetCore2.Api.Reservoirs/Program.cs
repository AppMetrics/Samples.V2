using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Filtering;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCore2.Api.Reservoirs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var filter = new MetricsFilter();
            filter.WhereContext(c => c == MetricsRegistry.Context);

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureMetricsWithDefaults(builder =>
                {
                    builder.Filter.With(filter);
                    builder.Report.ToInfluxDb("http://127.0.0.1:8086", "appmetricsreservoirs");
                })
                .UseMetrics()
                .UseStartup<Startup>()
                .Build();
        }
    }
}
