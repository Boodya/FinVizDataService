namespace StockMarketServiceDatabase.Models.Query
{
    public class UserQueryModel : StockDataQueryRequestModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string QueryTitle { get; set; }
        public int RevisionNumber { get; set; }
    }
}
