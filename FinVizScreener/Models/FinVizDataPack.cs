namespace FinVizDataService.Models
{
    public class FinVizDataPack
    {
        public DateTime FetchDate { get; set; }
        public IEnumerable<FinVizDataItem> Items { get; set; }
    }
}
