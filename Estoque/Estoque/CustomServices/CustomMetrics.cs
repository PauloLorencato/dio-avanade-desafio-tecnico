using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Estoque.CustomServices;

public class CustomMetrics
{
    private readonly Meter _meter = new Meter("CustomMetrics", "1.0");
    private readonly Process _process = Process.GetCurrentProcess();

    private double _lastCpuTime;
    private DateTime _lastMeasurement = DateTime.UtcNow;
    private long _lastCollectionCount = GC.CollectionCount(0);
    private DateTime _lastGCTime = DateTime.UtcNow;
    private int _requestCount = 0;

    public CustomMetrics()
    {
    }

    public void CreateMetrics()
    {
        // Métrica de uso de CPU
        _meter.CreateObservableGauge("cpu_usage_percentage", () =>
        {
            double cpuUsage = 0;
            var elapsed = DateTime.UtcNow - _lastMeasurement;
            if (elapsed.TotalMilliseconds > 0)
            {
                double currentCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
                cpuUsage = ((currentCpuTime - _lastCpuTime) / elapsed.TotalMilliseconds) / Environment.ProcessorCount * 100;
                _lastCpuTime = currentCpuTime;
            }
            _lastMeasurement = DateTime.UtcNow;
            return new[] { new Measurement<double>(cpuUsage) };
        }, "percent");

        // Métrica de uso de memória
        _meter.CreateObservableGauge("memory_usage_bytes", () =>
        {
            return new[] { new Measurement<long>(_process.WorkingSet64) };
        }, "bytes");

        // Métrica de requisições por minuto
        _meter.CreateObservableGauge("requests_per_minute", () =>
        {
            var elapsed = DateTime.UtcNow - _lastMeasurement;
            int requestsPerMinute = (int)(_requestCount / elapsed.TotalMinutes);
            _requestCount = 0;
            return new[] { new Measurement<int>(requestsPerMinute) };
        }, "requests");

        // Métrica da última coleta de GC
        _meter.CreateObservableGauge("last_gc_datetime", () =>
        {
            long currentCollectionCount = GC.CollectionCount(0);
            if (currentCollectionCount > _lastCollectionCount)
            {
                _lastGCTime = DateTime.UtcNow;
                _lastCollectionCount = currentCollectionCount;
            }

            var unixTimestamp = ((DateTimeOffset)_lastGCTime).ToUnixTimeSeconds();
            return new[] { new Measurement<long>(unixTimestamp) };
        }, "timestamp");
    }
}