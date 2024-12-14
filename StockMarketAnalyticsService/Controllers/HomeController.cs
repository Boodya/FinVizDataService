using LiteDB;
using Microsoft.AspNetCore.Mvc;
using StockMarketAnalyticsService.Services;
using StockMarketDataProcessing.Services;
using StockMarketServiceDatabase.Models.Query;
using StockMarketServiceDatabase.Services.User;

namespace StockMarketAnalyticsService.Controllers
{
    public class HomeController : AuthBaseController
    {
        private readonly StockScreenerService _stockScreenerService;
        private readonly FilterCalculationService _filterCalculationService;

        public HomeController(StockScreenerService stockScreenerService, 
            IUserDataService userDataService,
            FilterCalculationService filterCalculationService) : base(userDataService)
        {
            _stockScreenerService = stockScreenerService;
            _filterCalculationService = filterCalculationService;
        }

        public IActionResult Index()
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            return View();
        }

        public ActionResult List()
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var userId = GetContextUserId();
            return View(_userDataService.QueriesService
                .GetUserQueries(userId.Value));
        }

        public ActionResult Edit(int queryId)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            if (queryId == 0)
            {
                var nQuery = new UserQueryModel()
                {
                    UserId = GetContextUserId() ?? 0,
                };
                return View(nQuery);
            }
            return View(_userDataService.
                QueriesService.GetQuery(queryId));
        }

        [HttpPost]
        public ActionResult SaveUserQuery(UserQueryModel query)
        {
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

        public ActionResult Instrument(string ticker, bool isIgnoreEmptyProps=false)
        {
            if (NeedLogin())
                return RedirectToAction("Login");
            var instrument = _stockScreenerService
                .GetInstrumentByTicker(ticker, isIgnoreEmptyProps);
            if (instrument == null)
                return View("Index");

            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ?
                PartialView("_InstrumentDetailsPartial", instrument) :
                View("Instrument", instrument);
        }

        [HttpPost]
        public PartialViewResult CalculateQueryPart([FromBody] UserQueryModel query)
        {
            var result = _filterCalculationService.Calculate(query);
            result.Deals = result.Deals
                .OrderBy(d => d.Ticker).ToList();
            return PartialView("_FilterCalculationPart", result);
        }
    }
}
