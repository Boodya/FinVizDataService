namespace StockMarketServiceDatabase.Models.Processing
{
    public class FilterCalculationModel
    {
        public Dictionary<DateTime, decimal> CalculationHistory;
        public decimal? Result => CalculationHistory?
            .Values.LastOrDefault();
    }
}
