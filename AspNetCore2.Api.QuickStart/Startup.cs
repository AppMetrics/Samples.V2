using System;
using App.Metrics;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Tracking;
using App.Metrics.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore2.Api.QuickStart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Shouldn't have to register this explicitly, read on AddMetricsEndpoints(Configuration).
            services.Configure<MetricsEndpointsHostingOptions>(Configuration.GetSection(nameof(MetricsEndpointsHostingOptions)));

            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.ReadFrom(Configuration)
                .Report.ToConsole(TimeSpan.FromSeconds(2))
                .Build();

            services.AddMetrics(metrics);
            services.AddMetricsTrackingMiddleware(Configuration);
            services.AddMetricsEndpoints(Configuration);
            services.AddMetricsReportScheduler();
            services.AddMvc().AddMetrics();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetricsAllMiddleware();
            app.UseMetricsAllEndpoints();
            app.UseMvc();
        }
    }
}
