using FinVizDataService.Models;
using FinVizScreener.Scrappers;
using HtmlAgilityPack;

namespace FinVizScreener.Scrapers
{
    public abstract class ScraperBase : IScrapper
    {
        protected async Task<HtmlDocument> GetPageContentAsync(string url) =>
            new HtmlWeb().Load(url);

        public abstract IEnumerable<FinVizDataItem> ScrapeDataTable(string url);
        public abstract IAsyncEnumerable<FinVizDataItem> ScrapeDataTableAsync(string url);
        protected decimal? ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return null;
        }

        protected int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
