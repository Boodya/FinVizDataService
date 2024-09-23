namespace FinVizDataService.Models
{
    public class FinVizDataPack
    {
        public DateOnly FetchDate { get; set; }
        public IEnumerable<FinVizDataItem> Items { get; set; }
    }
}
