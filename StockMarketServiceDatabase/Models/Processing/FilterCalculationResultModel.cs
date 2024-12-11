namespace StockMarketServiceDatabase.Models.Processing
{
    public class FilterCalculationResultModel
    {
        public DateTime CalculationDate;
        public int QueryId;
        public int LastDataRevisionNum;
        public decimal AverageSuccessRate;
        public decimal AverageProfitRate;
        public decimal AverageLossRate;
        public int SuccessDeals;
        public int FailedDeals;
        public List<FilterCalculationDealModel> Deals;
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
