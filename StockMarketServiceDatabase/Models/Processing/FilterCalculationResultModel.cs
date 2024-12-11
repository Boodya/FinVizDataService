namespace StockMarketServiceDatabase.Models.Processing
{
    public class FilterCalculationResultModel
    {
        public DateTime CalculationDate;
        public int QueryId;
        public int LastDataRevisionNum;
        public decimal AverageSuccessRate;
        public decimal AverageProfitRate;
    }
}
