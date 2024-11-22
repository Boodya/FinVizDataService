using FinVizScreener.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Models;
using StockMarketAnalyticsService.Services;

namespace StockMarketAnalyticsService.Controllers
{
    public class UserQueriesController : Controller
    {
        private readonly StockScreenerService _stockScreenerService;
        private readonly IUserDataService _userDataService;

        public UserQueriesController(StockScreenerService stockScreenerService, IUserDataService userDataService)
        {
            _userDataService = userDataService;
            _stockScreenerService = stockScreenerService;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Authenticate(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                ViewBag.Message = "Invalid email address";
                return View("Login");
            }
            HttpContext.Session.SetString("Email", email);
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var email = HttpContext.Session.GetString("Email");
            return View(_userDataService
                .GetUserQueries(email).ToList());
        }

        public ActionResult Edit(int indx)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var email = HttpContext.Session.GetString("Email");
            var queries = _userDataService
                .GetUserQueries(email).ToList();
            return View(queries[indx]);
        }

        [HttpPost]
        public ActionResult Save(LinqProcessorRequestModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var email = HttpContext.Session.GetString("Email");
            _userDataService.SaveQuery(email, query);
            return RedirectToAction("List");
        }

        public ActionResult Delete(LinqProcessorRequestModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var email = HttpContext.Session.GetString("Email");
            _userDataService.DeleteQuery(email, query);
            return RedirectToAction("List");
        }

        public ActionResult Execute(LinqProcessorRequestModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var result = _stockScreenerService.QueryData(query);
            return View("ExecuteResult", result);
        }

        private bool NeedLogin()
        {
            var email = HttpContext.Session.GetString("Email");
            return string.IsNullOrEmpty(email);
        }
    }
}
