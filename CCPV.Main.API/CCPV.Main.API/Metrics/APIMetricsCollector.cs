using CCPV.Main.API.Handler;
using CCPV.Main.Background;
using Prometheus;

namespace CCPV.Main.API.Metrics
{
    public class APIMetricsCollector : MetricsCollector
    {
        private readonly IPortfolioHandler _portfolioHandler;

        public APIMetricsCollector(IPortfolioHandler portfolioHandler)
        {
            _portfolioHandler = portfolioHandler;
        }
        public static readonly Gauge PortfolioCount = Prometheus.Metrics.CreateGauge("portfolio_count", "Total portfolios in system");
        public static readonly Gauge UserCount = Prometheus.Metrics.CreateGauge("user_count", "Total registered users");
        public static readonly Gauge CachedCoinPrices = Prometheus.Metrics.CreateGauge("cached_coin_prices", "Number of coin prices cached");
        public static readonly Gauge PortfoliosRecalculated = Prometheus.Metrics.CreateGauge("portfolios_recalculated", "Portfolios recalculated in interval");

        // Call this periodically or on relevant events:
        public override void UpdateMetrics()
        {
            base.UpdateMetrics();
        }
        public void IncrementExceptionCount()
        {
            ApiErrorsTotal.Inc();
        }
    }
}
