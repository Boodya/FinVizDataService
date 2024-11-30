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
            var userId = GetContextUserId();
            if (userId == null)
                return RedirectToAction("Login");
            return View(_userDataService.QueriesService
                .GetUserQueries(userId.Value));
        }

        public ActionResult Edit(int queryId)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            return View(_userDataService.
                QueriesService.GetQuery(queryId));
        }

        [HttpPost]
        public ActionResult Save(UserQueryModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            _userDataService.QueriesService.AddOrUpdateQuery(query);
            return RedirectToAction("List");
        }

        public ActionResult Delete(UserQueryModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            _userDataService.QueriesService.DeleteQuery(query);
            return RedirectToAction("List");
        }

        public ActionResult Execute(UserQueryModel query)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var result = _stockScreenerService.QueryData(query.Query);
            return View("ExecuteResult", result);
        }

        private bool NeedLogin() =>
            GetContextUserLogin() == null;

        private string? GetContextUserLogin()
        {
            return HttpContext.Session.GetString("Email");
        }

        private int? GetContextUserId()
        {
            var userLogin = GetContextUserLogin();
            if (userLogin == null)
                return null;
            return _userDataService.GetUser(userLogin)?.Id;
        }
    }
}
