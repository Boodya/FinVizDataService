﻿using LiteDB;
using StockMarketDataProcessing.Models;
using StockMarketDataProcessing.Processors.FilterQuery;
using StockMarketServiceDatabase.Models.FinViz;
using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using StockMarketServiceDatabase.Services.FinViz;
using StockMarketServiceDatabase.Services.Query;
using System.Globalization;

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

        public FilterCalculationResultModel Calculate(UserQueryModel filter) =>
            CalculateFilter(filter, _finVizData.GetAllDataRevisions());

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
            List<int> dataRevisions)
        {
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
            return CalculateSummary(filter.Id, calculations);
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

        private FilterCalculationResultModel CalculateSummary(int queryId, List<FilterActionOnDataRevisionModel> revisions)
        {
            var result = new FilterCalculationResultModel()
            {
                QueryId = queryId,
                CalculationDate = DateTime.Now.ToUniversalTime(),
                LastDataRevisionNum = revisions.MaxBy(a => a.DataRevision)?
                    .DataRevision ?? 0,
                Deals = new List<FilterCalculationDealModel>()
            };
            CalculateDeals(queryId, result, revisions);
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
                else if(action.Action == OperationAction.Sell)
                {
                    var openedDeal = model.Deals.FirstOrDefault(
                        d => d.Ticker == action.Ticker && d.CloseRevNumber == 0);
                    if (openedDeal == null)
                        throw new Exception("Something wrong - attempted to close the deal that should be opened before");
                    openedDeal.CloseRevNumber = rev.DataRevision;
                    openedDeal.LastPrice = action.Price;
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
            });
        }
    }
}
