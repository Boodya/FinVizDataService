using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Services;

namespace StockMarketAnalyticsService.Controllers
{
    public class HomeController : Controller
    {
        private readonly StockScreenerService _stockScreenerService;

        public HomeController(StockScreenerService stockScreenerService)
        {
            _stockScreenerService = stockScreenerService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
