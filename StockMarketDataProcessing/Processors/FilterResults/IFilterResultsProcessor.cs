using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;

namespace StockMarketDataProcessing.Processors.FilterResults
{
    public interface IFilterResultsProcessor
    {
        public FilterCalculationResultModel Calculate(UserQueryModel query);
        public FilterCalculationResultModel Calculate(int queryId);
        public List<FilterCalculationResultModel> RecalculateAllQueries();
    }
}
