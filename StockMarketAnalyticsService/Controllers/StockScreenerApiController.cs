using FinVizDataService.Models;
using FinVizScreener.Services;
using Microsoft.AspNetCore.Mvc;

namespace StockMarketAnalyticsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockScreenerApiController : ControllerBase
    {
        private FinVizScrapperService _dataService;
        public StockScreenerApiController(ILogger<StockScreenerApiController> logger, 
            FinVizScrapperService ds)
        {
            _dataService = ds;
        }

        [Route("GetTickers")]
        [HttpGet]
        public ActionResult<List<string>> GetTickerList(string searchTerm = "")
        {
            var tickers = string.IsNullOrWhiteSpace(searchTerm)
                ? _dataService.Data.Select(item => item.Ticker)
                : _dataService.Data
                    .Where(item => item.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(item => item.Ticker);

            var tickerList = tickers
                .Distinct()
                .OrderBy(ticker => ticker)
                .ToList();

            return Ok(tickerList);
        }

        [Route("FindByExactTicker")]
        [HttpGet]
        public ActionResult<FinVizDataItem> GetInstrumentData(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest("Ticker cannot be empty.");
            }

            var result = _dataService.Data
                .FirstOrDefault(item => string.Equals(item.Ticker, ticker, StringComparison.OrdinalIgnoreCase));

            if (result == null)
            {
                return NotFound($"No data found for ticker '{ticker}'.");
            }

            return Ok(result);
        }

        [Route("SearchByTicker")]
        [HttpGet]
        public ActionResult<List<FinVizDataItem>> SearchByTicker(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            var result = _dataService.Data
                .Where(item => item.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Ticker)
                .ToList();

            return Ok(result);
        }

        [Route("FetchPaginatedData")]
        [HttpGet]
        public ActionResult<List<FinVizDataItem>> FetchPaginatedData(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than zero.");
            }
            int totalItems = _dataService.Data.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages)
            {
                return BadRequest("Requested page exceeds total number of pages.");
            }

            var paginatedData = _dataService.Data
                .OrderBy(i => i.Ticker)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return paginatedData;
        }

        [Route("FetchAllLatestData")]
        [HttpPost]
        public List<FinVizDataItem> FetchAllLatestData() =>
            _dataService.Data
                .OrderBy(i => i.Ticker)
                .ToList();
    }
}
