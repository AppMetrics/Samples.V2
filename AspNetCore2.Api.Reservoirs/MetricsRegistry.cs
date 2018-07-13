using System;
using App.Metrics;
using App.Metrics.Infrastructure;
using App.Metrics.ReservoirSampling.ExponentialDecay;
using App.Metrics.ReservoirSampling.SlidingWindow;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Scheduling;
using App.Metrics.Timer;

namespace AspNetCore2.Api.Reservoirs
{
    public static class MetricsRegistry
    {
        public static readonly string Context = "Reservoirs";

        public static TimerOptions TimerUsingAlgorithmRReservoir = new TimerOptions
                                                                   {
                                                                       Context = Context,
                                                                       Name = "uniform",
                                                                       Reservoir = () => new DefaultAlgorithmRReservoir()
                                                                   };

        public static TimerOptions TimerUsingExponentialForwardDecayingReservoir = new TimerOptions
                                                                                   {
                                                                                       Context = Context,
                                                                                       Name = "exponentially-decaying",
            Reservoir = () => new DefaultForwardDecayingReservoir(AppMetricsReservoirSamplingConstants.DefaultSampleSize, AppMetricsReservoirSamplingConstants.DefaultExponentialDecayFactor, 0.0, new StopwatchClock())
        };

        public static TimerOptions TimerUsingSlidingWindowReservoir = new TimerOptions
                                                                      {
                                                                          Context = Context,
                                                                          Name = "sliding-window",
                                                                          Reservoir = () => new DefaultSlidingWindowReservoir()
                                                                      };

        public static TimerOptions TimerUsingForwardDecayingLowWeightThresholdReservoir =
            new TimerOptions
            {
                Context = Context,
                Name = "exponentially-decaying-low-weight",
                Reservoir = () => new DefaultForwardDecayingReservoir(
                    AppMetricsReservoirSamplingConstants.DefaultSampleSize,
                    0.1, // Bias heavily towards lasst 15 seconds of sampling; disregard everything older than 40 seconds
                    0.001, // Samples with weight of less than 10% of average should be discarded when rescaling
                    new StopwatchClock(),
                    new DefaultReservoirRescaleScheduler(TimeSpan.FromSeconds(30)))
            };
    }
}