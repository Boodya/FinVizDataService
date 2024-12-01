using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Services;
using StockMarketServiceDatabase.Models;
using StockMarketServiceDatabase.Services;
using System.Reflection;

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
                return View(new UserQueryModel()
                {
                    UserId = GetContextUserId().Value,
                    QueryTitle = "TEST"
                });
            return View(_userDataService.
                QueriesService.GetQuery(queryId));
        }

        [HttpPost]
        public ActionResult SaveUserQuery(UserQueryModel query)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Error: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
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
            var sUid = HttpContext.Session.GetString("uId");
            return sUid == null ? null : int.Parse(sUid);
        }
    }
}
