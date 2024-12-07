using StockMarketServiceDatabase.Models;
using StockMarketServiceDatabase.Models.Processing;

namespace StockMarketDataProcessing.Processors
{
    public interface IFilterResultsProcessor
    {
        public FilterCalculationModel Calculate(LinqProcessorRequestModel filter);
    }
}
