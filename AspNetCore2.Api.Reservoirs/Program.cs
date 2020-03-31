using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Filtering;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AspNetCore2.Api.Reservoirs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IHost BuildWebHost(string[] args)
        {
            var filter = new MetricsFilter();
            filter.WhereContext(c => c == MetricsRegistry.Context);

            return Host.CreateDefaultBuilder(args)
                .ConfigureMetricsWithDefaults(builder =>
                {
                    builder.Filter.With(filter);
                    builder.Report.ToInfluxDb("http://127.0.0.1:8086", "appmetricsreservoirs", TimeSpan.FromSeconds(1));
                })
                .UseMetrics()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
        }
    }
}
