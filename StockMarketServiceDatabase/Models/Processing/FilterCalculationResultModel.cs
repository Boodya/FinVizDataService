using LiteDB;

namespace StockMarketServiceDatabase.Models.Processing
{
    public class FilterCalculationResultModel
    {
        public DateTime CalculationDate { get; set; }
        public int QueryId { get; set; }
        public int LastDataRevisionNum { get; set; }
        public decimal AverageSuccessRate { get; set; }
        public decimal AverageProfitRate { get; set; }
        public decimal AverageLossRate { get; set; }
        public int SuccessDeals { get; set; }
        public int FailedDeals { get; set; }
        [BsonIgnore]
        public List<FilterCalculationDealModel> Deals { get; set; }
        [BsonIgnore]
        public string CalculationError { get; set; }
    }

    public class FilterCalculationDealModel
    {
        public string Ticker;
        public int QueryId;
        public decimal EntryPrice;
        public decimal LastPrice;
        public int EntryRevNumber;
        public int CloseRevNumber;
    }
}
