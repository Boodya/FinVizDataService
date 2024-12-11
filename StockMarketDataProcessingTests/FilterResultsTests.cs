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
                new LocalLiteDBSeparateFilesAdapter("D:\\Temp\\FinVizDataServiceDB\\FinVizData\\data"),
                new LiteDBUserQueriesService("D:\\Temp\\FinVizDataServiceDB\\FinVizData\\userData"),
                new MapBasedLinqQueryProcessor<FinVizDataItem>("ItemProperties"));
        }

        [Test]
        public void CalculateAllQueriesTest()
        {
           
            var calculations = _processor.CalculateAllQueries();
            Assert.True(calculations.Any());
        }
    }
}