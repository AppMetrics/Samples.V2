﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using App.Metrics.Scheduling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore2.Api.Reservoirs
{
    public class Startup
    {
        public static readonly Uri ApiBaseAddress = new Uri("http://localhost:5000/");

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMetrics();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RunRequestsToSample();

            app.UseMvc();
        }

        private static void RunRequestsToSample()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = ApiBaseAddress
            };

            var requestSamplesScheduler = new AppMetricsTaskScheduler(TimeSpan.FromMilliseconds(10), async () =>
            {
                var uniform = httpClient.GetStringAsync("api/reservoirs/uniform");
                var exponentiallyDecaying = httpClient.GetStringAsync("api/reservoirs/exponentially-decaying");
                var exponentiallyDecayingLowWeight = httpClient.GetStringAsync("api/reservoirs/exponentially-decaying-low-weight");
                var slidingWindow = httpClient.GetStringAsync("api/reservoirs/sliding-window");

                await Task.WhenAll(uniform, exponentiallyDecaying, exponentiallyDecayingLowWeight, slidingWindow);
            });

            requestSamplesScheduler.Start();
        }
    }
}
