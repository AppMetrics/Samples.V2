using System.IO;
using System.Text;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Timer;
using static System.Console;

namespace Net452.Metrics.Console.QuickStart
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var metrics = AppMetrics.CreateDefaultBuilder().Build();

            var counter = new CounterOptions { Name = "my_counter" };
            metrics.Measure.Counter.Increment(counter);

            var gauge = new GaugeOptions {Name = "my_gauge"};
            metrics.Measure.Gauge.SetValue(gauge, 1);

            var meter = new MeterOptions { Name = "my_meter" };
            metrics.Measure.Meter.Mark(meter);

            var histogram = new HistogramOptions { Name = "my_histogram" };
            metrics.Measure.Histogram.Update(histogram, 10);

            var timer = new TimerOptions { Name = "my_timer" };
            using (metrics.Measure.Timer.Time(timer))
            {
                await Task.Delay(100);
            }

            var apdex = new ApdexOptions { Name = "my_apdex", AllowWarmup = false, ApdexTSeconds = 0.1 };
            using (metrics.Measure.Apdex.Track(apdex))
            {
                await Task.Delay(200);
            }

            var snapshot = metrics.Snapshot.Get();

            using (var stream = new MemoryStream())
            {
                await metrics.DefaultOutputMetricsFormatter.WriteAsync(stream, snapshot);
                var result = Encoding.UTF8.GetString(stream.ToArray());
                WriteLine(result);
            }

            ReadKey();
        }
    }
}
