using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;

namespace StockMarketDataProcessing.Processors.FilterResults
{
    public interface IFilterResultsProcessor
    {
        public FilterCalculationModel Calculate(StockDataQueryRequestModel filter);
    }
}
