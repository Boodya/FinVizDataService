using FinVizScreener.Models;
using HtmlAgilityPack;

namespace FinVizScreener.Scrapers
{
    public class StockOverviewTableScrapper : ScraperBase
    {
        public async Task<List<StockOverviewModel>> GetStocksList(string baseUrl)
        {
            return await ScrapeStocksPage(baseUrl, new List<StockOverviewModel>(), 1);
        }

        private async Task<List<StockOverviewModel>> ScrapeStocksPage(string baseUrl, 
            List<StockOverviewModel> stocks, int pageParam)
        {
            var pageStocks = await ScrapeStocks($"{baseUrl}{ScrapersConsts.PagingParam}{pageParam}");
            if (pageStocks.Count == 0)
                return stocks;
            var lastNum = pageStocks.LastOrDefault()?.TableNo;
            if (stocks.Any(s => s.TableNo == lastNum))
                return stocks;
            stocks.AddRange(pageStocks);
            pageParam = lastNum.Value + 1;
            return await ScrapeStocksPage(baseUrl, stocks, pageParam);
        }

        private async Task<List<StockOverviewModel>> ScrapeStocks(string url)
        {
            var htmlContent = await GetPageContentAsync(url);
            return string.IsNullOrEmpty(htmlContent) ?
                new List<StockOverviewModel>() : ScrapeHtml(htmlContent);
        }

        protected List<StockOverviewModel> ScrapeHtml(string htmlContent)
        {
            var stocks = new List<StockOverviewModel>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var tableRows = htmlDoc.DocumentNode.SelectNodes("//tr[@class='styled-row is-hoverable is-bordered is-rounded is-striped has-color-text']");

            if (tableRows != null)
            {
                foreach (var row in tableRows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells != null && cells.Count >= 10)
                    {
                        var tickerNode = cells[1].SelectSingleNode(".//a[@class='tab-link']");
                        var link = tickerNode?.GetAttributeValue("href", string.Empty);
                        var stock = new StockOverviewModel
                        {
                            TableNo = ParseInt(cells[0]?.InnerText.Trim()),
                            Ticker = tickerNode?.InnerText.Trim(),
                            Url = "https://finviz.com/" + link,
                            Company = cells[2]?.InnerText.Trim(),
                            Sector = cells[3]?.InnerText.Trim(),
                            Industry = cells[4]?.InnerText.Trim(),
                            Country = cells[5]?.InnerText.Trim(),
                            MarketCap = cells[6]?.InnerText.Trim(),
                            PERatio = cells[7]?.InnerText.Trim(),
                            Price = cells[8]?.InnerText.Trim(),
                            Change = cells[9]?.InnerText.Trim(),
                            Volume = cells[10]?.InnerText.Trim()
                        };
                        stocks.Add(stock);
                    }
                }
            }
            return stocks;
        }
    }
}
