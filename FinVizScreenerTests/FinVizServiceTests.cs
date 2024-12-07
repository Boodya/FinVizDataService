using StockMarketServiceDatabase.Models.FinViz;
using FinVizScreener.Services;

namespace FinVizScreenerTests
{
    public class FinVizServiceTests
    {
        [Fact]
        public async void OnServiceDataDownloadedTest()
        {
            var service = new FinVizScrapperService(new FinVizDataServiceConfigModel()
            {
                EndpointUrl = TestsConfig.ScrappingUrl,
                DatabaseConnectionString = TestsConfig.LiteDBConnectionString,
                DatabaseType = "LiteDB",
                ExecutionTime = DateTime.Now.AddSeconds(1).TimeOfDay,
            });
            List<FinVizDataItem> fetchedData = new();
            service.Subscribe(fetchedData.Add);
            while (fetchedData.Count <= 8000)
                Thread.Sleep(1000);
            Assert.True(fetchedData != null);
            Assert.True(fetchedData.Count >= 8000);
        }
    }
}
