using FinVizDataService.Models;
using FinVizScreener.DB;
using FinVizScreener.Services;

namespace FinVizScreenerTests
{
    public class FinVizServiceTests
    {
        [Fact]
        public async void OnServiceDataDownloadedTest()
        {
            var service = new FinvizScheduledScrapperService(new FinVizScreener.Models.FinVizDataServiceConfigModel()
            {
                EndpointUrl = TestsConfig.ScrappingUrl,
                DatabaseConnectionString = "litedb-test.db",
                DatabaseType = "LiteDB",
                StartTime = TimeSpan.FromHours(08),
                DataFetchPeriod = TimeSpan.FromDays(1),
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
