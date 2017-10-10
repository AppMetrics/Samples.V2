using App.Metrics.ReservoirSampling.ExponentialDecay;
using App.Metrics.ReservoirSampling.SlidingWindow;
using App.Metrics.ReservoirSampling.Uniform;
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
                                                                                       Reservoir = () => new DefaultForwardDecayingReservoir()
                                                                                   };

        public static TimerOptions TimerUsingSlidingWindowReservoir = new TimerOptions
                                                                      {
                                                                          Context = Context,
                                                                          Name = "sliding-window",
                                                                          Reservoir = () => new DefaultSlidingWindowReservoir()
                                                                      };
    }
}