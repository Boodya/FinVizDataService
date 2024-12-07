using StockMarketServiceDatabase.Models.FinViz;
using FinVizScreener.Scrapers;

namespace FinVizScreener.Scrappers
{
    public class PaginatedFullScrapper : ScraperBase
    {
        private TimeSpan _scrapeDelay = TimeSpan.FromMilliseconds(200);
        private OnePageScrapper _scrapper;
        public PaginatedFullScrapper()
        {
            _scrapper = new OnePageScrapper();
        }
        public override IEnumerable<FinVizDataItem> ScrapeDataTable(string url)
        {
            var result = new List<FinVizDataItem>();
            var itemNum = 1;
            while (true)
            {
                var pageData = ScrapePage(_scrapper, url, itemNum);
                result.AddRange(pageData);
                if (pageData.Count() < Consts.PageItemLimit)
                    break;
                itemNum += Consts.PageItemLimit;
                Thread.Sleep(_scrapeDelay);
            }
            return result;
        }

        public override async IAsyncEnumerable<FinVizDataItem> ScrapeDataTableAsync(string url)
        {
            var itemNum = 1;
            while (true)
            {
                var pageData = await Task.Run(() => 
                    ScrapePage(_scrapper, url, itemNum));
                foreach (var item in pageData)
                {
                    yield return item;
                }
                if (pageData.Count() < Consts.PageItemLimit)
                    break;
                itemNum += Consts.PageItemLimit;
                await Task.Delay(_scrapeDelay);
            }
        }


        private IEnumerable<FinVizDataItem> ScrapePage(IScrapper scrapper, string url, int itemNum)
        {
            var scrapeUrl = $"{url}{Consts.PagingFilter}{itemNum}";
            return _scrapper.ScrapeDataTable(scrapeUrl);
        }
    }
}
