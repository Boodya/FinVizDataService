using FinVizDataService.Models;
using FinVizScreener.DB;
using Microsoft.AspNetCore.Mvc;

namespace FinVizDataService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockScreenerApiController : ControllerBase
    {
        private IFinvizDBAdapter _dbAdapter;
        public StockScreenerApiController(ILogger<StockScreenerApiController> logger, 
            IFinvizDBAdapter db)
        {
            _dbAdapter = db;
        }

        [Route("FetchAllLatestData")]
        [HttpPost]
        public List<FinVizDataItem> FetchAllLatestData() =>
            _dbAdapter.GetLatestData().ToList();
    }
}
