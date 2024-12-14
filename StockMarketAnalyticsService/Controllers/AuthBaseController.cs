using Microsoft.AspNetCore.Mvc;
using StockMarketServiceDatabase.Models.User;
using StockMarketServiceDatabase.Services.User;

namespace StockMarketAnalyticsService.Controllers
{
    public class AuthBaseController : Controller
    {
        protected readonly IUserDataService _userDataService;
        public AuthBaseController(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        public ActionResult SignOut()
        {
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("uId");
            return RedirectToAction("Login");
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

        protected bool NeedLogin() =>
            GetContextUserLogin() == null;

        protected string? GetContextUserLogin()
        {
            return HttpContext.Session.GetString("Email");
        }

        protected int? GetContextUserId()
        {
            var sUid = HttpContext.Session.GetString("uId");
            return sUid == null ? null : int.Parse(sUid);
        }
    }
}
