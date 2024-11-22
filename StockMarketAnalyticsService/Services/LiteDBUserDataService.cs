using LiteDB;
using StockMarketAnalyticsService.Models;

namespace StockMarketAnalyticsService.Services
{
    public class LiteDBUserDataService : IUserDataService
    {
        private readonly LiteDatabase _db;

        public LiteDBUserDataService(string dbConnectionString)
        {
            _db = new LiteDatabase(dbConnectionString);
        }

        public UserQueryModel GetUser(string email)
        {
            var users = _db.GetCollection<UserQueryModel>("users");
            return users.FindOne(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public void SetUser(UserQueryModel user)
        {
            var users = _db.GetCollection<UserQueryModel>("users");

            users.EnsureIndex(u => u.Email);

            var existingUser = users.FindOne(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
            if (existingUser == null)
            {
                user.Id = Guid.NewGuid().ToString();
                users.Insert(user);
            }
            else
            {
                user.Id = existingUser.Id;
                users.Update(user);
            }
        }

        public List<LinqProcessorRequestModel> GetUserQueries(string email) =>
            GetUser(email)?.Queries ?? new List<LinqProcessorRequestModel>();

        public void SaveQuery(string email, LinqProcessorRequestModel query)
        {
            var users = _db.GetCollection<UserQueryModel>("users");
            var user = users.FindOne(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                throw new InvalidOperationException($"User with email '{email}' not found.");
            }
            user.Queries.Add(query);
            users.Update(user);
        }

        public void DeleteQuery(string email, LinqProcessorRequestModel query)
        {
            throw new NotImplementedException();
        }
    }

}
