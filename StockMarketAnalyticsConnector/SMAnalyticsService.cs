using StockMarketAnalyticsConnector.Connectors;

namespace StockMarketAnalyticsConnector
{
    public class SMAnalyticsService
    {
        public StockScreenerApiConnector StockScreener { get; private set; }
        private readonly string _enpointUrl;
        public SMAnalyticsService(string endpointUrl) 
        {
            _enpointUrl = endpointUrl;
        }
    }
}
