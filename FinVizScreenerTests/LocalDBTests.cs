using FinVizDataService.Models;
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
            var dbAdapter = DBAdapterFactory.Resolve("LiteDB",
                TestsConfig.LiteDBConnectionString);
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

        [Fact]
        public void LocalLiteDBLoadSaveTest()
        {
            var dbAdapter = DBAdapterFactory.Resolve("LiteDB",
                TestsConfig.LiteDBConnectionString);
            var loadedData = dbAdapter.GetLatestData();
            var result = dbAdapter.SaveData(loadedData);
            Assert.Equal(0, result);
        }

        [Fact]
        public void LocalLiteDBLoadChangeSaveTest()
        {
            var dbAdapter = DBAdapterFactory.Resolve("LiteDB",
                TestsConfig.LiteDBConnectionString);
            var loadedData = dbAdapter.GetLatestData();
            var itemToOperate = loadedData.FirstOrDefault();
            Assert.NotNull(itemToOperate);

            foreach (var propKey in itemToOperate.ItemProperties.Keys)
            {
                itemToOperate.ItemProperties[propKey] = "TEST";
            }
            var beforeVersion = itemToOperate.Version;
            var beforeTicker = itemToOperate.Ticker;
            var savedCount = dbAdapter.SaveData(new List<FinVizDataItem>() { itemToOperate });
            Assert.Equal(1, savedCount);
            var afterItems = dbAdapter.GetLatestData();
            var afterItem = afterItems.Where(i => i.Ticker == beforeTicker).FirstOrDefault();
            Assert.NotNull(afterItem);
            Assert.True(beforeVersion != afterItem.Version);
        }
    }
}
