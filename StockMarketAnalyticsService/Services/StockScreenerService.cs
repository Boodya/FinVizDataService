using FinVizDataService.Models;
using FinVizScreener.Services;
using StockMarketAnalyticsService.Models;
using StockMarketAnalyticsService.QueryProcessors;

namespace StockMarketAnalyticsService.Services
{
    public class StockScreenerService
    {
        private readonly FinVizScrapperService _dataService;
        private readonly MapBasedLinqQueryProcessor<FinVizDataItem> _queryProcessor;

        public StockScreenerService(FinVizScrapperService dataService)
        {
            _dataService = dataService;
            _queryProcessor = new MapBasedLinqQueryProcessor<FinVizDataItem>("ItemProperties");
        }

        public List<FinVizDataItem> QueryData(LinqProcessorRequestModel query)
        {
            return _queryProcessor.QueryData(_dataService.Data, query);
        }

        public List<string> GetTickerList(string searchTerm = "")
        {
            var tickers = string.IsNullOrWhiteSpace(searchTerm)
                ? _dataService.Data.Select(item => item.Ticker)
                : _dataService.Data
                    .Where(item => item.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(item => item.Ticker);

            return tickers.Distinct().OrderBy(ticker => ticker).ToList();
        }

        public List<FinVizDataItem> SearchByTicker(string searchTerm, List<string> propsFilter = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty.");

            return GetFilteredData(propsFilter)
                .Where(item => item.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Ticker)
                .ToList();
        }

        public List<FinVizDataItem> FetchPaginatedData(int page, int pageSize, List<string> propsFilter = null)
        {
            if (page <= 0 || pageSize <= 0)
                throw new ArgumentException("Page and pageSize must be greater than zero.");

            var data = GetFilteredData(propsFilter);
            int totalItems = data.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages)
                throw new ArgumentException("Requested page exceeds total number of pages.");

            return data.OrderBy(i => i.Ticker)
                       .Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();
        }

        public List<FinVizDataItem> GetStocksData(List<string> propsFilter = null)
        {
            return GetFilteredData(propsFilter);
        }

        private List<FinVizDataItem> GetFilteredData(List<string> propsFilter = null)
        {
            if (propsFilter == null)
                return _dataService.Data.OrderBy(i => i.Ticker).ToList();

            return _dataService.Data
                .Select(item => new FinVizDataItem
                {
                    Id = item.Id,
                    Version = item.Version,
                    Date = item.Date,
                    Ticker = item.Ticker,
                    ItemProperties = item.ItemProperties
                        .Where(kv => propsFilter.Any(key =>
                            string.Equals(key, kv.Key, StringComparison.OrdinalIgnoreCase)))
                        .ToDictionary(kv => kv.Key, kv => kv.Value)
                })
                .ToList();
        }
    }
}
