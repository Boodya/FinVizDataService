﻿using FinVizDataService.Models;
using FinVizScreener.Scrapers;

namespace FinVizScreener.Scrappers
{
    public class FullScrapper : ScraperBase
    {
        public override IEnumerable<FinVizDataItem> ScrapeDataTable(string url)
        {
            var htmlPage = base.GetPageContentAsync(url).Result;
            var dataItems = new List<FinVizDataItem>();
            var tableNode = htmlPage.DocumentNode.SelectSingleNode("//table[contains(@class, 'screener_table')]");
            if (tableNode != null)
            {
                var headerNodes = tableNode.SelectNodes(".//thead/tr/th");

                var headers = new List<string>();

                foreach (var headerNode in headerNodes)
                {
                    headers.Add(headerNode.InnerText.Trim());
                }

                var rows = tableNode.SelectNodes(".//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count != headers.Count) 
                        continue;

                    var itemProperties = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        itemProperties[headers[i]] = cells[i].InnerText.Trim();
                    }

                    var dataItem = new FinVizDataItem
                    {
                        Id = int.Parse(cells[0].InnerText.Trim()),
                        Ticker = cells[1].InnerText.Trim(),
                        ItemProperties = itemProperties
                    };

                    dataItems.Add(dataItem);
                }
            }

            return dataItems;
        }
    }
}
