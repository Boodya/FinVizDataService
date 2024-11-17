using FinVizDataService.Models;
using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Models;
using StockMarketAnalyticsService.Services;

namespace StockMarketAnalyticsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockScreenerApiController : ControllerBase
    {
        private readonly StockScreenerService _stockScreenerService;

        public StockScreenerApiController(StockScreenerService stockScreenerService)
        {
            _stockScreenerService = stockScreenerService;
        }

        [Route("Query")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> Query(LinqProcessorRequestModel query)
        {
            try
            {
                return Ok(_stockScreenerService.QueryData(query));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error on processing query: {ex.Message}");
            }
        }

        [Route("GetTickers")]
        [HttpPost]
        public ActionResult<List<string>> GetTickerList(string searchTerm = "")
        {
            return Ok(_stockScreenerService.GetTickerList(searchTerm));
        }

        [Route("SearchByTicker")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> SearchByTicker(string searchTerm, List<string> propsFilter = null)
        {
            try
            {
                return Ok(_stockScreenerService.SearchByTicker(searchTerm, propsFilter));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("FetchPaginatedData")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> FetchPaginatedData(int page = 1, int pageSize = 10, List<string> propsFilter = null)
        {
            try
            {
                return Ok(_stockScreenerService.FetchPaginatedData(page, pageSize, propsFilter));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("StocksData")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> GetStocksData(List<string> propsFilter = null)
        {
            return Ok(_stockScreenerService.GetStocksData(propsFilter));
        }
    }
}
