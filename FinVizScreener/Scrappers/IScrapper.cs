using FinVizDataService.Models;

namespace FinVizScreener.Scrappers
{
    public interface IScrapper
    {
        IEnumerable<FinVizDataItem> ScrapeDataTable(string url);
        IAsyncEnumerable<FinVizDataItem> ScrapeDataTableAsync(string url);
    }
}
