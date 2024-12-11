using StockMarketDataProcessing.Models;
using StockMarketDataProcessing.Processors.FilterQuery;
using StockMarketServiceDatabase.Models.FinViz;
using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using StockMarketServiceDatabase.Services.FinViz;
using StockMarketServiceDatabase.Services.Query;
using System.Buffers;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public FilterCalculationResultModel Calculate(StockDataQueryRequestModel filter)
        {
            return null;
        }

        public List<FilterCalculationResultModel> CalculateAllQueries()
        {
            var result = new List<FilterCalculationResultModel>();
            var dataRevisions = _finVizData.GetAllDataRevisions();
            _queries.IterateQueries((batch) =>
            {
                
                foreach (var query in batch)
                {
                    var calculations = new List<FilterActionOnDataRevisionModel>();
                    dataRevisions.ForEach(rev =>
                    {
                        var data = _finVizData.GetRevision(rev).ToList();
                        var queriedData = _queryProcessor.QueryData(
                            data,
                            query);
                        ///TODO: IMPLEMENT SELL CALCULATION
                        /*calculations.LastOrDefault()?.Actions.ForEach(action =>
                        {
                            var tickerToSell = queriedData.FirstOrDefault(q => q.Ticker == action.Ticker)?.Ticker;
                            if (tickerToSell != null)
                            {
                                calculations.Add(new()
                                {
                                    DataRevision = rev,
                                    QueriedData = queriedData,
                                    Actions = new List<ActionModel>() { new ActionModel()
                                        {
                                            Action = OperationAction.Sell,
                                            Ticker = tickerToSell,
                                            Price = GetPrice(data, tickerToSell)
                                        } 
                                    }
                                });
                            }
                        });*/
                        calculations.Add(
                            CalculateActions(rev, queriedData, data));
                    });
                    result.Add(CalculateSummary(query.Id, calculations));
                }
            });
            return result;
        }

        private FilterActionOnDataRevisionModel CalculateActions(int dataRevision, 
            List<FinVizDataItem> queriedData, List<FinVizDataItem> data)
        {
            var result = new FilterActionOnDataRevisionModel()
            {
                DataRevision = dataRevision,
                QueriedData = queriedData,
                Actions = new()
            };
            foreach (var queryData in queriedData)
            {
                result.Actions.Add(new()
                {
                    Action = OperationAction.Buy,
                    Ticker = queryData.Ticker,
                    Price = GetPrice(data, queryData.Ticker)
                });
            }
            return result;
        }

        private decimal GetPrice(List<FinVizDataItem> data, string ticker)
        {
            decimal price = 0;
            var dataItem = data.FirstOrDefault(d => d.Ticker == ticker);
            if (dataItem != null)
            {
                var priceStr = dataItem.ItemProperties
                    .FirstOrDefault(prop => prop.Key == "Price").Value;

                if (!string.IsNullOrEmpty(priceStr))
                    price = Convert.ToDecimal(priceStr);
            }
            return price;
        }

        private FilterCalculationResultModel CalculateSummary(int queryId, List<FilterActionOnDataRevisionModel> actions)
        {
            var result = new FilterCalculationResultModel()
            {
                QueryId = queryId,
                CalculationDate = DateTime.Now.ToUniversalTime(),
                LastDataRevisionNum = actions.MaxBy(a => a.DataRevision)?.DataRevision ?? 0
            };

            return result;
        }
    }
}
