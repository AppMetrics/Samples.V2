﻿using System;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore2.Api.Reservoirs.Controllers
{
    [Route("api/[controller]")]
    public class ReservoirsController : Controller
    {
        private readonly IMetrics _metrics;

        public ReservoirsController(IMetrics metrics)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        [HttpGet("exponentially-decaying")]
        public async Task<string> ExponentiallyDecaying()
        {
            using (_metrics.Measure.Timer.Time(MetricsRegistry.TimerUsingExponentialForwardDecayingReservoir))
            {
                await Delay();
            }

            return "OK";
        }

        [HttpGet("exponentially-decaying-low-weight")]
        public async Task<string> ExponentiallyDecayingLowWeight()
        {
            using (_metrics.Measure.Timer.Time(MetricsRegistry.TimerUsingForwardDecayingLowWeightThresholdReservoir))
            {
                await Delay();
            }

            return "OK";
        }

        [HttpGet("sliding-window")]
        public async Task<string> SlidingWindow()
        {
            using (_metrics.Measure.Timer.Time(MetricsRegistry.TimerUsingSlidingWindowReservoir))
            {
                await Delay();
            }

            return "OK";
        }

        [HttpGet("uniform")]
        public async Task<string> Uniform()
        {
            using (_metrics.Measure.Timer.Time(MetricsRegistry.TimerUsingAlgorithmRReservoir))
            {
                await Delay();
            }

            return "OK";
        }

        private Task Delay()
        {
            var second = DateTime.Now.Second;

            if (second <= 20)
            {
                return Task.CompletedTask;
            }

            if (second <= 40)
            {
                return Task.Delay(TimeSpan.FromMilliseconds(50), HttpContext.RequestAborted);
            }

            return Task.Delay(TimeSpan.FromMilliseconds(100), HttpContext.RequestAborted);
        }
    }
}