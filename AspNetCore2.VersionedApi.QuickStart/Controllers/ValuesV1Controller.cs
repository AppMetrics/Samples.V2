using System.Collections.Generic;
using App.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore2.VersionedApi.QuickStart.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/values")]
    public class ValuesV1Controller : Controller
    {
        private readonly IMetrics _metrics;

        public ValuesV1Controller(IMetrics metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _metrics.Measure.Counter.Increment(MetricsRegistry.SampleCounter);

            return new[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
