using FinVizDataService.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinVizDataService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FinVizScrapperController : ControllerBase
    {
        public FinVizScrapperController(ILogger<FinVizScrapperController> logger)
        {
        }

        [HttpGet(Name = "GetFullData")]
        public List<FinVizDataItem> Get()
        {
            return new List<FinVizDataItem>();
        }
    }
}
