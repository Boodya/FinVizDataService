using FinVizScreener.DB;
using FinVizScreener.Scrappers;

namespace FinVizScreenerTests
{
    public class LocalDBTests
    {
        public LocalDBTests()
        {
            
        }

        [Fact]
        public void LocalLiteDBSaveLoadTest()
        {
            var dbAdapter = new LocalLiteDBFinvizAdapter("litedb-test");
            var scrapper = new OnePageScrapper();
            var data = scrapper.ScrapeDataTable(TestsConfig.ScrappingUrl);
            var originalItem = data.FirstOrDefault();
            Assert.NotNull(originalItem);
            dbAdapter.SaveData(data);

            var loadedData = dbAdapter.GetLatestData();
            var dbProcessedItem = loadedData
                .Where(d => d.Ticker == originalItem.Ticker)
                .FirstOrDefault();
            Assert.NotNull(dbProcessedItem);

            foreach(var propKey in originalItem.ItemProperties.Keys)
            {
                Assert.True(originalItem.ItemProperties[propKey] ==  dbProcessedItem.ItemProperties[propKey]);
            }
        }
    }
}
