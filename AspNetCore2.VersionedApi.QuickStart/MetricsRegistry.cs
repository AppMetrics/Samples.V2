﻿using App.Metrics;
using App.Metrics.Counter;

namespace AspNetCore2.VersionedApi.QuickStart
{
    public static class MetricsRegistry
    {
        public static CounterOptions SampleCounter => new CounterOptions
        {
            Name = "Sample Counter",
            MeasurementUnit = Unit.Calls
        };
    }
}
