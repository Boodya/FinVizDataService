using FinVizDataService.Models;
using FinVizScreener.Services;
using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Models;
using StockMarketAnalyticsService.QueryProcessors;

namespace StockMarketAnalyticsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockScreenerApiController : ControllerBase
    {
        private FinVizScrapperService _dataService;
        private MapBasedLinqQueryProcessor<FinVizDataItem> _queryProcessor;
        public StockScreenerApiController(ILogger<StockScreenerApiController> logger, 
            FinVizScrapperService ds)
        {
            _dataService = ds;
            _queryProcessor = new MapBasedLinqQueryProcessor<FinVizDataItem>("ItemProperties");
        }

        [Route("Query")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> Query(LinqProcessorRequestModel query)
        {
            try
            {
                return Ok(_queryProcessor.QueryData(_dataService.Data, query));
            }
            catch(Exception ex)
            {
                return BadRequest($"Error on processing query: {ex.Message}");
            }
        }

        [Route("GetTickers")]
        [HttpPost]
        public ActionResult<List<string>> GetTickerList(string searchTerm = "")
        {
            var tickers = string.IsNullOrWhiteSpace(searchTerm)
                ? _dataService.Data.Select(item => item.Ticker)
                : _dataService.Data
                    .Where(item => item.Ticker
                        .Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(item => item.Ticker);

            var tickerList = tickers
                .Distinct()
                .OrderBy(ticker => ticker)
                .ToList();

            return Ok(tickerList);
        }

        [Route("SearchByTicker")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> SearchByTicker(string searchTerm,
            List<string> propsFilter = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty.");
            return Ok(GetFilteredData(propsFilter)
                .Where(item => item.Ticker
                    .Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Ticker)
                .ToList());
        }

        [Route("FetchPaginatedData")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> FetchPaginatedData(int page = 1, int pageSize = 10,
            List<string> propsFilter = null)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than zero.");
            var data = GetFilteredData(propsFilter);
            int totalItems = data.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages)
                return BadRequest("Requested page exceeds total number of pages.");

            var paginatedData = data
                .OrderBy(i => i.Ticker)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return paginatedData;
        }

        [Route("StocksData")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> GetStocksData(List<string> propsFilter = null) =>
            Ok(GetFilteredData(propsFilter));

        private List<FinVizDataItem> GetFilteredData(List<string> propsFilter = null)
        {
            if (propsFilter == null)
                return _dataService.Data
                .OrderBy(i => i.Ticker)
                .ToList();

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
