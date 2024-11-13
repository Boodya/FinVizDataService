using FinVizDataService.Models;

namespace FinVizScreener.DB
{
    public interface IFinvizDBAdapter
    {
        public IEnumerable<FinVizDataItem> GetLatestData();
        public int SaveData(IEnumerable<FinVizDataItem> data);
    }
}
