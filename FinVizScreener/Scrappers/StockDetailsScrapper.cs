using FinVizScreener.Helpers;
using FinVizScreener.Models;
using HtmlAgilityPack;
using System.Reflection;

namespace FinVizScreener.Scrapers
{
    public class StockDetailsScrapper : ScraperBase
    {
        public async Task<StockDetailedModel> GetStockDetails(StockOverviewModel overviewModel)
        {
            var htmlContent = await GetPageContentAsync(overviewModel.Url);
            return string.IsNullOrEmpty(htmlContent) ?
                new StockDetailedModel() : ScrapeHtml(htmlContent, overviewModel);
        }

        protected StockDetailedModel ScrapeHtml(string htmlContent, StockOverviewModel? overviewModel = null)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var tableRows = htmlDoc.DocumentNode.SelectNodes("//table[@class='js-snapshot-table snapshot-table2 screener_snapshot-table-body']//tr");
            var stockDetails = overviewModel == null ? new StockDetailedModel() :
                new StockDetailedModel(overviewModel);
            if (tableRows != null)
            {
                foreach (var row in tableRows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells != null && cells.Count >= 10)
                    {
                        var properties = typeof(StockDetailedModel).GetProperties();

                        foreach (var prop in properties)
                        {
                            var attribute = prop.GetCustomAttribute<MapToTitleAttribute>();
                            if (attribute != null)
                            {
                                var title = attribute.Title;
                                var cell = cells.FirstOrDefault(c => c.InnerText.Trim() == title);
                                if (cell != null)
                                {
                                    var value = cells[cells.IndexOf(cell) + 1]?.InnerText.Trim();
                                    prop.SetValue(stockDetails, value);
                                }
                            }
                        }
                    }
                }
            }
            return stockDetails;
        }
    }
}
