using LiteDB;

namespace StockMarketServiceDatabase.Models.Processing
{
    public class FilterCalculationResultModel
    {
        public DateTime CalculationDate { get; set; }
        [BsonId]
        public int QueryId { get; set; }
        public string Filter { get; set; }
        public int LastDataRevisionNum { get; set; }
        public decimal AverageSuccessRate { get; set; }
        public decimal AverageProfitRate { get; set; }
        public decimal AverageLossRate { get; set; }
        public int SuccessDeals { get; set; }
        public int FailedDeals { get; set; }
        public List<FilterCalculationDealModel> Deals { get; set; }
        public List<FilterCalculationTickerDealsModel> TickerDeals { get; set; }
        public string CalculationError { get; set; }
    }

    public class FilterCalculationTickerDealsModel
    {
        public string Ticker { get; set; }
        public int QueryId { get; set; }
        public int SuccessDeals { get; set; }
        public int FailedDeals { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageProfitRate { get; set; }
        public List<FilterCalculationDealModel> Deals { get; set; }
    }

    public class FilterCalculationDealModel
    {
        public string Ticker { get; set; }
        public int QueryId { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal ProfitPercent { get; set; }
        public int EntryRevNumber { get; set; }
        public int CloseRevNumber { get; set; }
    }
}
