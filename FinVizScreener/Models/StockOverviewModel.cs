namespace FinVizScreener.Models
{
    public class StockOverviewModel : IStockModel
    {
        public int TableNo { get; set; }
        public string Ticker { get; set; }
        public string Url { get; set; }
        public string Company { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; }
        public string MarketCap { get; set; }
        public string PERatio { get; set; }
        public string Price { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
    }
}
