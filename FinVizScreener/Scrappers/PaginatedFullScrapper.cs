using FinVizDataService.Models;
using FinVizScreener.Scrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinVizScreener.Scrappers
{
    public class PaginatedFullScrapper : ScraperBase
    {
        private TimeSpan _scrapeDelay = TimeSpan.FromMilliseconds(200);
        private FullScrapper _scrapper;
        public PaginatedFullScrapper()
        {
            _scrapper = new FullScrapper();
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

        private IEnumerable<FinVizDataItem> ScrapePage(IScrapper scrapper, string url, int itemNum)
        {
            var scrapeUrl = $"{url}{Consts.PagingFilter}{itemNum}";
            return _scrapper.ScrapeDataTable(scrapeUrl);
        }
    }
}
