namespace StockMarketAnalyticsService.Models
{
    public class LinqProcessorRequestModel
    {
        public string Filter { get; set; }
        public string Sort { get; set; }
        public string Select { get; set; }
        public int? Top { get; set; }
    }
}
