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

        [Route("GetSupportedTickers")]
        [HttpPost]
        public ActionResult<List<string>> GetTickerList()
        {
            return Ok(_stockScreenerService
                .GetTickerList());
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

        [Route("PaginatedQuery")]
        [HttpPost]
        public ActionResult<PaginatedQueryResponseModel<FinVizDataItem>> FetchPaginatedData(int page = 1, int pageSize = 10, LinqProcessorRequestModel? query = null)
        {
            try
            {
                return Ok(_stockScreenerService.FetchPaginatedData(page, pageSize, query));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
