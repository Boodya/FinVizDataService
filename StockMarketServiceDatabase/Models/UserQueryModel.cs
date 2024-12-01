namespace StockMarketServiceDatabase.Models
{
    public class UserQueryModel
    {
        public int UserId { get; set; }
        public int QueryId { get; set; }
        public string QueryTitle { get; set; }
        public LinqProcessorRequestModel Query { get; set; }

        public UserQueryModel()
        {
            Query = new();
        }
    }
}
