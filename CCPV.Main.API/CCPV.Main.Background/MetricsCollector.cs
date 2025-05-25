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

            // Example: ActiveConnections could be set from DB connection pool info
            // ActiveConnections.Set(GetActiveDbConnections());

            // Example: Query database for counts
            // PortfolioCount.Set(GetPortfolioCountFromDb());
            // UserCount.Set(GetUserCountFromDb());

            // Example: CachedCoinPrices - update from cache service
            // CachedCoinPrices.Set(GetCachedCoinPricesCount());

            // PortfoliosRecalculated - increment or set after recalculation
            // PortfoliosRecalculated.Set(GetRecalculatedCount());
        }
    }
}
