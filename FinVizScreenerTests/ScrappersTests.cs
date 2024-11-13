using FinVizDataService.Models;
using FinVizScreener.Scrappers;

namespace FinVizScreenerTests
{
    public class ScrappersTests
    {
        public ScrappersTests() 
        {
            
        }
        [Fact]
        public async void OnePageScrapeTestAsync()
        {
            var scrapper = new OnePageScrapper();
            var result = scrapper.ScrapeDataTableAsync(TestsConfig.ScrappingUrl);
            var items = new List<FinVizDataItem>();
            await foreach (var item in result)
            {
                items.Add(item);
            }
            Assert.NotNull(items);
            Assert.Equal(20, items.Count);
        }

        [Fact]
        public async void AllDataScrapperTestAsync()
        {
            var scrapper = new PaginatedFullScrapper();
            var result = scrapper.ScrapeDataTableAsync(TestsConfig.ScrappingUrl);
            var items = new List<FinVizDataItem>();
            await foreach (var item in result)
            {
                items.Add(item);
            }
            Assert.NotNull(items);
            Assert.True(items.Count > 8000);
        }
    }
}