using FinVizDataService.Models;

namespace FinVizScreener.DB
{
    public interface IFinvizDBAdapter
    {
        public FinVizDataPack GetLatestData();
        public void SaveData(FinVizDataPack data);
    }
}
