using LiteDB;

namespace StockMarketServiceDatabase.Models.User
{
    public class UserModel
    {
        [BsonId]
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
