using StockMarketServiceDatabase.Models.FinViz;
using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Services;
using StockMarketServiceDatabase.Models.Query;

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

        [Route("GetTickers")]
        [HttpGet]
        public ActionResult<List<string>> GetTickerList(string matchingPattern = "") =>
            _stockScreenerService
                .GetTickerList(matchingPattern);

        [Route("SearchTickerCompanyList")]
        [HttpGet]
        public ActionResult<Dictionary<string, string>> SearchTickerCompanyList(string matchingPattern = "") =>
            _stockScreenerService.SearchTickerCompanyNameMap(matchingPattern);

        [Route("Query")]
        [HttpPost]
        public ActionResult<List<FinVizDataItem>> Query(StockDataQueryRequestModel query)
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
        public ActionResult<PaginatedQueryResponseModel<FinVizDataItem>> FetchPaginatedData(int page = 1, int pageSize = 10, StockDataQueryRequestModel? query = null)
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
