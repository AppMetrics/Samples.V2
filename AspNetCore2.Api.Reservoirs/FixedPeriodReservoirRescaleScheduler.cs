using System;
using System.Collections.Concurrent;
using System.Threading;
using App.Metrics.ReservoirSampling;
using App.Metrics.Scheduling;
using Validation;

namespace AspNetCore2.Api.Reservoirs
{
    // Implmentation taken from https://github.com/karolz-ms/AirTrafficControl/blob/master/atc.utilities/AppMetrics/ForwardDecayingLowWeightThresholdReservoir.cs
    // Added to test before PR https://github.com/AppMetrics/AppMetrics/issues/260
    public class FixedPeriodReservoirRescaleScheduler : IReservoirRescaleScheduler
    {
        private readonly ConcurrentBag<IRescalingReservoir> _reservoirs;
        private readonly Timer _rescalingTimer;
        private bool _isDisposed;
        private readonly TimeSpan _rescalePeriod;

        public FixedPeriodReservoirRescaleScheduler(TimeSpan rescalePeriod)
        {
            Requires.That(rescalePeriod >= TimeSpan.FromSeconds(1),
                          nameof(rescalePeriod),
                          "Rescale period of {0:c} is too small (must be at least 1 second)",
                          rescalePeriod);

            _rescalePeriod = rescalePeriod;
            _isDisposed = false;
            _reservoirs = new ConcurrentBag<IRescalingReservoir>();
            _rescalingTimer = new Timer(DoRescaling, null, rescalePeriod, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _rescalingTimer.Dispose();
        }

        public void RemoveSchedule(IRescalingReservoir reservoir)
        {
            Requires.NotNull(reservoir, nameof(reservoir));

            _reservoirs.TryTake(out IRescalingReservoir unused);
        }

        public void ScheduleReScaling(IRescalingReservoir reservoir)
        {
            Requires.NotNull(reservoir, nameof(reservoir));
            Verify.NotDisposed(!_isDisposed, $"{nameof(FixedPeriodReservoirRescaleScheduler)} was disposed");

            _reservoirs.Add(reservoir);
        }

        private void DoRescaling(object state)
        {
            // It is safe to iterate over ConcurrentBag, even when it is being concurrently modified
            foreach (var reservoir in _reservoirs)
            {
                reservoir.Rescale();
            }

            _rescalingTimer.Change(_rescalePeriod, Timeout.InfiniteTimeSpan);
        }
    }
}
