using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using App.Metrics;
using App.Metrics.Concurrency;
using App.Metrics.ReservoirSampling;
using App.Metrics.ReservoirSampling.ExponentialDecay;
using App.Metrics.Scheduling;
using Validation;

namespace AspNetCore2.Api.Reservoirs
{
    // Implmentation taken from https://github.com/karolz-ms/AirTrafficControl/blob/master/atc.utilities/AppMetrics/ForwardDecayingLowWeightThresholdReservoir.cs
    // Added to test before PR https://github.com/AppMetrics/AppMetrics/issues/260
    public class ForwardDecayingLowWeightThresholdReservoir : IRescalingReservoir, IDisposable
    {
        private static readonly string ReservoirDisposedMessage = $"{nameof(ForwardDecayingLowWeightThresholdReservoir)} was disposed";
        private static readonly string LockNotTaken = "Operation failed because the reservoir could not ensure exclusive access to internal data structures";

        private readonly double _alpha;
        private readonly double _sampleWeightThreshold;
        private readonly IClock _clock;
        private readonly IReservoirRescaleScheduler _rescaleScheduler;
        private readonly int _sampleSize;
        private SortedList<double, WeightedSample> _values;

        private long _count = 0L;
        private bool _disposed = false;
        private SpinLock _lock = new SpinLock(enableThreadOwnerTracking: false);
        private long _startTime;
        private double _sum = 0.0;


        public ForwardDecayingLowWeightThresholdReservoir(
            int sampleSize,
            double alpha,
            double sampleWeightThreshold,
            IClock clock,
            IReservoirRescaleScheduler rescaleScheduler)
        {
            _sampleSize = sampleSize;
            _alpha = alpha;
            _sampleWeightThreshold = sampleWeightThreshold;
            _clock = clock;
            _rescaleScheduler = rescaleScheduler;

            _values = new SortedList<double, WeightedSample>(sampleSize, ReverseOrderDoubleComparer.Instance);
            _startTime = clock.Seconds;
            _rescaleScheduler.ScheduleReScaling(this);
        }

        public void Dispose()
        {
            _disposed = true;
            _rescaleScheduler.RemoveSchedule(this);
        }

        public IReservoirSnapshot GetSnapshot(bool resetReservoir)
        {
            Verify.NotDisposed(!_disposed, ReservoirDisposedMessage);

            WeightedSnapshot snapshot = null;

            ExecuteAsCriticalSection(() => {
                snapshot = new WeightedSnapshot(_count, _sum, _values.Values);
                if (resetReservoir)
                {
                    ResetReservoir();
                }
            });

            return snapshot;
        }

        public IReservoirSnapshot GetSnapshot() => GetSnapshot(false);

        public void Rescale()
        {
            Verify.NotDisposed(!_disposed, ReservoirDisposedMessage);

            ExecuteAsCriticalSection(() => {
                var oldStartTime = _startTime;
                _startTime = _clock.Seconds;

                var scalingFactor = Math.Exp(-_alpha * (_startTime - oldStartTime));

                var newSamples = new Dictionary<double, WeightedSample>(_values.Count);

                foreach (var keyValuePair in _values)
                {
                    var sample = keyValuePair.Value;

                    var newWeight = sample.Weight * scalingFactor;
                    if (newWeight < _sampleWeightThreshold)
                    {
                        continue;
                    }

                    var newKey = keyValuePair.Key * scalingFactor;
                    var newSample = new WeightedSample(sample.Value, sample.UserValue, sample.Weight * scalingFactor);
                    newSamples.Add(newKey, newSample);
                }

                _values = new SortedList<double, WeightedSample>(newSamples, ReverseOrderDoubleComparer.Instance);

                // Need to reset the samples counter after rescaling
                _count = _values.Count;
                _sum = _values.Values.Aggregate(0L, (current, sample) => current + sample.Value);
            });
        }

        public void Reset()
        {
            ExecuteAsCriticalSection(() => ResetReservoir());
        }

        public void Update(long value, string userValue)
        {
            Update(value, userValue, _clock.Seconds);
        }

        public void Update(long value)
        {
            Update(value, null, _clock.Seconds);
        }

        private void Update(long value, string userValue, long timestamp)
        {
            Verify.NotDisposed(!_disposed, ReservoirDisposedMessage);

            var itemWeight = Math.Exp(_alpha * (timestamp - _startTime));
            var sample = new WeightedSample(value, userValue, itemWeight);

            var random = 0.0;

            // Prevent division by 0
            // TODO: what about underflow?
            while (random.Equals(0.0))
            {
                random = ThreadLocalRandom.NextDouble();
            }

            var priority = itemWeight / random;

            ExecuteAsCriticalSection(() => {
                _count++;
                _sum += value;

                if (_count <= _sampleSize)
                {
                    _values[priority] = sample;
                }
                else
                {
                    var first = _values.Keys[_values.Count - 1];
                    if (first < priority)
                    {
                        _values.Remove(first);
                        _values[priority] = sample;
                    }
                }
            });
        }

        private void ResetReservoir()
        {
            _values.Clear();
            _count = 0L;
            _sum = 0.0;
            _startTime = _clock.Seconds;
        }

        private void ExecuteAsCriticalSection(Action action)
        {
            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                if (!lockTaken)
                {
                    throw new InvalidOperationException(LockNotTaken);
                }

                action();
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
        }

        private sealed class ReverseOrderDoubleComparer : IComparer<double>
        {
            public static readonly IComparer<double> Instance = new ReverseOrderDoubleComparer();

            public int Compare(double x, double y) { return y.CompareTo(x); }
        }
    }
}
