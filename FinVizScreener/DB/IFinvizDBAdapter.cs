using FinVizDataService.Models;

namespace FinVizScreener.DB
{
    public interface IFinvizDBAdapter
    {
        public IEnumerable<FinVizDataItem> GetLatestData();
        public void SaveData(IEnumerable<FinVizDataItem> data);
    }
}
