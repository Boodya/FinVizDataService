using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Services;
using StockMarketServiceDatabase.Models;
using StockMarketServiceDatabase.Services;

namespace StockMarketAnalyticsService.Controllers
{
    public class HomeController : Controller
    {
        private readonly StockScreenerService _stockScreenerService;
        private readonly IUserDataService _userDataService;

        public HomeController(StockScreenerService stockScreenerService, IUserDataService userDataService)
        {
            _userDataService = userDataService;
            _stockScreenerService = stockScreenerService;
        }

        public IActionResult Index()
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            return View();
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
            var user = _userDataService.GetUser(email);
            if (user == null)
            {
                user = new UserModel() { Email = email };
                user.Id = _userDataService.AddOrUpdateUser(user);
            }
            HttpContext.Session.SetString("uId", user.Id.ToString());
            HttpContext.Session.SetString("Email", user.Email);
            return RedirectToAction("Index");
        }

        public ActionResult SignOut()
        {
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("uId");
            return RedirectToAction("Login");
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
            var isValid = ModelState.IsValid;
            if (NeedLogin())
                return RedirectToAction("Login");
            if (queryId == 0)
            {
                var nQuery = new UserQueryModel()
                {
                    UserId = GetContextUserId().Value,
                    QueryTitle = "TEST"
                };
                return View(nQuery);
            }
            return View(_userDataService.
                QueriesService.GetQuery(queryId));
        }

        [HttpPost]
        public ActionResult SaveUserQuery(UserQueryModel query)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", query);
            }
            if (NeedLogin())
                return RedirectToAction("Login");
            _userDataService.QueriesService.AddOrUpdateQuery(query);
            return RedirectToAction("List");
        }

        public ActionResult Delete(int queryId)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            _userDataService.QueriesService.DeleteQuery(
                _userDataService.QueriesService.GetQuery(queryId));
            return RedirectToAction("List");
        }

        public ActionResult Execute(int queryId)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var query = _userDataService
                .QueriesService.GetQuery(queryId);
            if (query == null)
                return View("List");
            var result = _stockScreenerService.QueryData(query);
            return View("QueryResults", result);
        }

        public ActionResult Instrument(string ticker)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var instruments = _stockScreenerService.QueryData(new()
            {
                Filter = $"[ticker] = {ticker}"
            });
            if (instruments == null)
                return View("Index");
            return View(instruments.FirstOrDefault());
        }

        private bool NeedLogin() =>
            GetContextUserLogin() == null;

        private string? GetContextUserLogin()
        {
            return HttpContext.Session.GetString("Email");
        }

        private int? GetContextUserId()
        {
            var sUid = HttpContext.Session.GetString("uId");
            return sUid == null ? null : int.Parse(sUid);
        }
    }
}
