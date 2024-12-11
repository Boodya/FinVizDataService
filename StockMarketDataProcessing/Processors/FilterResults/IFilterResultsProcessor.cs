using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;

namespace StockMarketDataProcessing.Processors.FilterResults
{
    public interface IFilterResultsProcessor
    {
        public FilterCalculationResultModel Calculate(StockDataQueryRequestModel filter);
        public List<FilterCalculationResultModel> CalculateAllQueries();
    }
}
