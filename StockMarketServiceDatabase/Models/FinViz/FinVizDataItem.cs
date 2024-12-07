namespace StockMarketServiceDatabase.Models.FinViz
{
    public class FinVizDataItem
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public DateTime Date { get; set; }
        public string Ticker { get; set; }
        public Dictionary<string, string> ItemProperties { get; set; }
    }
}
