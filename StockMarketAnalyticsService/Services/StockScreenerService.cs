using StockMarketServiceDatabase.Models.FinViz;
using FinVizScreener.Services;
using StockMarketDataProcessing.Processors.FilterQuery;
using StockMarketServiceDatabase.Models.Query;
using System.Linq;

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

        public FinVizDataItem? GetInstrumentByTicker(string ticker, bool isIgnoreEmptyProperties=false)
        {
            var instrument = _queryProcessor.QueryData(_dataService.Data, new()
            {
                Filter = $"[ticker] = {ticker}"
            }).FirstOrDefault();
            if(isIgnoreEmptyProperties && instrument != null)
            {
                var propMap = new Dictionary<string, string>();
                instrument.ItemProperties = instrument.ItemProperties.Where(p => !string
                    .IsNullOrEmpty(p.Value) && !p.Value.Contains("-")).ToDictionary();
            }
            return instrument;
        }

        public List<FinVizDataItem> QueryData(StockDataQueryRequestModel query)
        {
            return _queryProcessor.QueryData(_dataService.Data, query);
        }

        public Dictionary<string, string> SearchTickerCompanyNameMap(string searchTerm = "")
        {
            var dataFetch = _dataService.Data as IEnumerable<FinVizDataItem>;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                dataFetch = dataFetch.Where(d =>
                    d.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    d.ItemProperties.Any(prop =>
                        prop.Key.Equals("Company", StringComparison.OrdinalIgnoreCase) &&
                        prop.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            return dataFetch.ToDictionary(
                d => d.Ticker,
                d => d.ItemProperties.FirstOrDefault(prop =>
                    prop.Key.Equals("Company", StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty
            );
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

        public PaginatedQueryResponseModel<FinVizDataItem> FetchPaginatedData(int page, int pageSize, StockDataQueryRequestModel? query = null)
        {
            if (page <= 0 || pageSize <= 0)
                return default;
            var data = _dataService.Data;
            if (query != null)
                data = QueryData(query);

            int totalItems = data.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages)
                return default;

            var dataSet = data.Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();
            return new PaginatedQueryResponseModel<FinVizDataItem>(page, pageSize, data.Count, dataSet);
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
