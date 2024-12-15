using LiteDB;
using StockMarketDataProcessing.Models;
using StockMarketDataProcessing.Processors.FilterQuery;
using StockMarketServiceDatabase.Models.FinViz;
using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using StockMarketServiceDatabase.Services.FinViz;
using StockMarketServiceDatabase.Services.Query;

namespace StockMarketDataProcessing.Processors.FilterResults
{
    public class FinVizDataIncrementalFilterProcessor : IFilterResultsProcessor
    {
        private IFinvizDBAdapter _finVizData;
        private IUserQueriesDataService _queries;
        private MapBasedLinqQueryProcessor<FinVizDataItem> _queryProcessor;
        public FinVizDataIncrementalFilterProcessor(
            IFinvizDBAdapter finVizData,
            IUserQueriesDataService queries,
            MapBasedLinqQueryProcessor<FinVizDataItem> queryProcessor)
        {
            _finVizData = finVizData;
            _queries = queries;
            _queryProcessor = queryProcessor;
        }

        public FilterCalculationResultModel Calculate(int queryId) =>
            Calculate(_queries.GetQuery(queryId));

        public FilterCalculationResultModel Calculate(UserQueryModel query) =>
            CalculateFilter(query, _finVizData.GetAllDataRevisions());

        public List<FilterCalculationResultModel> RecalculateAllQueries()
        {
            var result = new List<FilterCalculationResultModel>();
            var dataRevisions = _finVizData.GetAllDataRevisions();
            var lastRevision = dataRevisions.Last();
            var queriesToProcess = _queries.GetQuery(
                q => q.RevisionNumber < lastRevision);
            queriesToProcess.ForEach(query =>
            {
                result.Add(
                    CalculateFilter(
                        query, dataRevisions));
            });
            return result;
        }

        private FilterCalculationResultModel CalculateFilter(UserQueryModel filter,
            List<int> dataRevisions, bool forceCalculate = false)
        {
            if (string.IsNullOrEmpty(filter.Filter))
                return new();
            var lastRevision = dataRevisions.Last();
            var calculation = TryNotRecalculate(filter, lastRevision, forceCalculate);
            if (calculation != null)
                return calculation;

            var calculations = new List<FilterActionOnDataRevisionModel>();
            filter.Top = 0;
            filter.Page = 0;

            dataRevisions.ForEach(rev =>
            {
                var data = _finVizData.GetRevision(rev).ToList();
                var queriedData = _queryProcessor.QueryData(
                    data,
                    filter);
                AddActions(rev, calculations,
                    queriedData, data);
            });
            calculation = CalculateSummary(filter, calculations);
            if (filter.Id != 0)
                _queries.AddOrUpdateQueryCalculation(calculation);
            return calculation;
        }

        private void AddActions(int dataRevision,
            List<FilterActionOnDataRevisionModel> calculations,
            List<FinVizDataItem> queriedData, List<FinVizDataItem> data)
        {
            var result = new FilterActionOnDataRevisionModel()
            {
                DataRevision = dataRevision,
                QueriedData = queriedData,
                Actions = new()
            };
            var prevCalculation = calculations.LastOrDefault();
            prevCalculation?.Actions.ForEach(act =>
            {
                if (act.Action == OperationAction.Sell)
                    return;

                if (!queriedData.Any(d => d.Ticker == act.Ticker))
                {
                    result.Actions.Add(new()
                    {
                        Action = OperationAction.Sell,
                        Ticker = act.Ticker,
                        Price = GetPrice(data, act.Ticker)
                    });
                }
            });
            foreach (var queryData in queriedData)
            {

                result.Actions.Add(new()
                {
                    Action = OperationAction.Buy,
                    Ticker = queryData.Ticker,
                    Price = GetPrice(data, queryData.Ticker)
                });
            }
            calculations.Add(result);
        }

        private decimal GetPrice(List<FinVizDataItem> data, string ticker)
        {
            decimal price = 0;
            var dataItem = data.FirstOrDefault(d => d.Ticker == ticker);
            if (dataItem != null)
            {
                var priceStr = dataItem.ItemProperties
                    .FirstOrDefault(prop => prop.Key == "Price").Value;
                price = Helpers.ToDecimal(priceStr);
            }
            return price;
        }

        private FilterCalculationResultModel CalculateSummary(UserQueryModel query, 
            List<FilterActionOnDataRevisionModel> revisions)
        {
            var result = new FilterCalculationResultModel()
            {
                QueryId = query.Id,
                Filter = query.Filter,
                CalculationDate = DateTime.Now.ToUniversalTime(),
                LastDataRevisionNum = revisions.MaxBy(a => a.DataRevision)?
                    .DataRevision ?? 0,
                Deals = new(),
                TickerDeals = new(),
                CalculationError = ""
            };
            CalculateDeals(query.Id, result, revisions);
            CalculateTickerDeals(result);
            var successDeals = 0;
            decimal totalProfit = 0;
            decimal totalLoss = 0;
            result.Deals.ForEach(d =>
            {
                var profit = Helpers.Percent(
                    d.LastPrice - d.EntryPrice, d.EntryPrice);
                totalProfit += profit;
                if (profit > 0)
                {
                    totalProfit += profit;
                    successDeals++;
                }
                else
                {
                    totalLoss += profit;
                }

            });
            result.SuccessDeals = successDeals;
            result.FailedDeals = result.Deals.Count - result.SuccessDeals;
            result.AverageSuccessRate = Helpers.Percent(successDeals, result.Deals.Count);
            result.AverageProfitRate = totalProfit / result.SuccessDeals;
            result.AverageLossRate = totalLoss / result.SuccessDeals;
            return result;
        }

        private void CalculateTickerDeals(FilterCalculationResultModel model)
        {
            model.TickerDeals = model.Deals
                .GroupBy(deal => new { deal.Ticker })
                .Select(group => new FilterCalculationTickerDealsModel
                {
                    Ticker = group.Key.Ticker,
                    QueryId = group.First().QueryId,
                    SuccessDeals = group.Count(deal => deal.ProfitPercent > 0),
                    FailedDeals = group.Count(deal => deal.ProfitPercent <= 0),
                    SuccessRate = group.Any() ? Helpers.Percent(group
                        .Count(deal => deal.ProfitPercent > 0), group.Count()) : 0,
                    AverageProfitRate = group.Sum(g => g.ProfitPercent) / group.Count(),
                    Deals = group.ToList()
                })
                .ToList();
        }
        private void CalculateDeals(int queryId, FilterCalculationResultModel model,
            List<FilterActionOnDataRevisionModel> revisions)
        {
            revisions.ForEach(rev => rev.Actions.ForEach(action =>
            {
                if (action.Action == OperationAction.Buy)
                {
                    if (!model.Deals.Any(d => d.CloseRevNumber == 0 && d.Ticker == action.Ticker))
                    {
                        model.Deals.Add(new()
                        {
                            QueryId = queryId,
                            Ticker = action.Ticker,
                            EntryRevNumber = rev.DataRevision,
                            EntryPrice = action.Price
                        });
                    }
                }
                else if (action.Action == OperationAction.Sell)
                {
                    var openedDeal = model.Deals.FirstOrDefault(
                        d => d.Ticker == action.Ticker && d.CloseRevNumber == 0);
                    if (openedDeal == null)
                        throw new Exception("Something wrong - attempted to close the deal that should be opened before");
                    openedDeal.CloseRevNumber = rev.DataRevision;
                    openedDeal.LastPrice = action.Price;
                    openedDeal.ProfitPercent = Helpers.Percent(
                        openedDeal.LastPrice - openedDeal.EntryPrice, openedDeal.EntryPrice);
                }
            }));
            var latestPrices = _queryProcessor.QueryData(_finVizData.GetRevision().ToList(), new()
            {
                Select = "Price"
            });
            model.Deals.Where(d => d.LastPrice == 0).ToList().ForEach(d =>
            {
                var lastPrice = latestPrices
                    .FirstOrDefault(p => p.Ticker == d.Ticker)?
                        .ItemProperties.FirstOrDefault(p => p.Key == "Price").Value;
                d.LastPrice = Helpers
                    .ToDecimal(lastPrice);
                d.ProfitPercent = Helpers.Percent(
                        d.LastPrice - d.EntryPrice, d.EntryPrice);
            });
        }

        private FilterCalculationResultModel? TryNotRecalculate(
            UserQueryModel filter, int lastRevNum, bool forceCalculate = false)
        {
            if (forceCalculate)
                return null;
            var existingQuery = _queries.GetQuery(filter.Id);
            var allQueries = _queries.GetQueryCalculations();
            var existingQueryCalculation = _queries.GetQueryCalculations(c =>
                c.QueryId == filter.Id).FirstOrDefault();
            if (existingQueryCalculation == null || existingQuery == null)
                return null;

            if (filter.Filter != existingQueryCalculation.Filter || 
                existingQueryCalculation.LastDataRevisionNum != lastRevNum)
                return null;

            return existingQueryCalculation;
        }
    }
}
