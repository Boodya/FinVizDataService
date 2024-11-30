namespace StockMarketAnalyticsService.Models
{
    public class UserQueryModel
    {
        public int UserId { get; set; }
        public int QueryId { get; set; }
        public int QueryTitle { get; set; }
        public LinqProcessorRequestModel Query { get; set; }
    }
}
