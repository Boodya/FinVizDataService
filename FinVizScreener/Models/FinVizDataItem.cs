namespace FinVizDataService.Models
{
    public class FinVizDataItem
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public Dictionary<string, string> ItemProperties { get; set; }
    }
}
