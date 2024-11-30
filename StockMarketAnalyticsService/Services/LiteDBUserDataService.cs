using LiteDB;
using StockMarketAnalyticsService.Models;

namespace StockMarketAnalyticsService.Services
{
    public class LiteDBUserDataService : IUserDataService
    {
        private string _databasePath;
        private const string _usersCollection = "Users";
        private IUserQueriesService _queriesService;
        public IUserQueriesService QueriesService => _queriesService;

        public LiteDBUserDataService(string dbPath)
        {
            _databasePath = dbPath;
            _queriesService = new LiteDBUserQueriesService(dbPath);
        }

        public UserModel GetUser(string email)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var users = db.GetCollection<UserModel>(_usersCollection);
                return users.FindOne(u => u.Email == email);
            }
        }

        public int AddOrUpdateUser(UserModel user)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var users = db.GetCollection<UserModel>(_usersCollection);

                users.EnsureIndex(u => u.Id, true);

                if (user.Id == 0)
                {
                    return users.Insert(user);
                }
                else
                {
                    users.Update(user);
                    return user.Id;
                }
            }
        }

        public UserModel GetUserById(int userId)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var users = db.GetCollection<UserModel>(_usersCollection);
                return users.FindById(userId);
            }
        }

        public void DeleteUser(int userId)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var users = db.GetCollection<UserModel>(_usersCollection);
                var userQueries = _queriesService.GetUserQueries(userId);
                users.Delete(userId);
                userQueries.ForEach(
                    _queriesService.DeleteQuery);
            }
        }
    }

}
