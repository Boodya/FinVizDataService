using StockMarketServiceDatabase.Models.FinViz;

namespace StockMarketServiceDatabase.Services.FinViz
{
    public interface IFinvizDBAdapter
    {
        public List<int> GetAllDataRevisions();
        Dictionary<string, FinVizDataItem> GetTickerMappedData(int version=0);
        public IEnumerable<FinVizDataItem> GetRevision(int version=0);
        public int SaveData(IEnumerable<FinVizDataItem> data);
    }
}
