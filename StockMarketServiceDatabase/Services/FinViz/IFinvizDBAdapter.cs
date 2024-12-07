using StockMarketServiceDatabase.Models.FinViz;

namespace StockMarketServiceDatabase.Services.FinViz
{
    public interface IFinvizDBAdapter
    {
        public IEnumerable<FinVizDataItem> GetLatestData();
        public int SaveData(IEnumerable<FinVizDataItem> data);
    }
}
