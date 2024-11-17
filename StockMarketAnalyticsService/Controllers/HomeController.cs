using Microsoft.AspNetCore.Mvc;

namespace StockMarketAnalyticsService.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
