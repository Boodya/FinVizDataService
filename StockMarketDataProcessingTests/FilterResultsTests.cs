using StockMarketDataProcessing.Processors.FilterQuery;
using StockMarketDataProcessing.Processors.FilterResults;
using StockMarketServiceDatabase.Models.FinViz;
using StockMarketServiceDatabase.Services.FinViz;
using StockMarketServiceDatabase.Services.Query;

namespace StockMarketDataProcessingTests
{
    public class Tests
    {
        private IFilterResultsProcessor _processor;

        [SetUp]
        public void Setup()
        {
            _processor = new FinVizDataIncrementalFilterProcessor(
                new LocalLiteDBSeparateFilesAdapter("E:\\LiteDB\\StockMarketAnalyticsService\\FinVizData"),
                new LiteDBUserQueriesService("E:\\LiteDB\\StockMarketAnalyticsService\\UserData"),
                new MapBasedLinqQueryProcessor<FinVizDataItem>("ItemProperties"));
        }

        [Test]
        public void CalculateAllQueriesTest()
        {
           
            var calculations = _processor.RecalculateAllQueries();
            Assert.True(calculations.Any());
        }
    }
}