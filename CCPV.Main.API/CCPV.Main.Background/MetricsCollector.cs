using Prometheus;

namespace CCPV.Main.Background
{
    public abstract class MetricsCollector
    {
        protected readonly Gauge ThreadCount = Metrics.CreateGauge("app_thread_count", "Number of active threads");
        protected readonly Gauge ActiveConnections = Metrics.CreateGauge("app_active_connections", "Number of active DB/API connections");
        protected readonly Counter ApiErrorsTotal = Metrics.CreateCounter("api_errors_total", "Total API errors");

        public virtual void UpdateMetrics()
        {
            ThreadCount.Set(System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
        }
    }
}
