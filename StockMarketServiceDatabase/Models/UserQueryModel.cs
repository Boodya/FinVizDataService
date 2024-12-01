namespace StockMarketServiceDatabase.Models
{
    public class UserQueryModel : LinqProcessorRequestModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string QueryTitle { get; set; }
    }
}
