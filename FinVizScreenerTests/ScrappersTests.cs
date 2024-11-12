using FinVizDataService.Models;
using FinVizScreener.Scrappers;

namespace FinVizScreener.Scrappers
{
    public class ScrappersTests
    {
        private string _scrapeUrl = "https://finviz.com/screener.ashx?v=152&c=0,1,2,79,3,4,5,6,7,8,9,10,11,12,13,73,74,75,14,15,16,77,17,18,19,20,21,23,22,82,78,127,128,24,25,85,26,27,28,29,30,31,84,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,125,126,59,68,70,80,83,76,60,61,62,63,64,67,69,81,86,87,88,65,66,103,100,107,108,109,112,113,114,115,116,117,120,121,122,105";
        public ScrappersTests() 
        {
            
        }
        [Fact]
        public async void OnePageScrapeTestAsync()
        {
            var scrapper = new OnePageScrapper();
            var result = scrapper.ScrapeDataTableAsync(_scrapeUrl);
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
            var result = scrapper.ScrapeDataTableAsync(_scrapeUrl);
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