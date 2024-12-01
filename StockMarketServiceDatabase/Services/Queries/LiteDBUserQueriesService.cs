using LiteDB;
using StockMarketServiceDatabase.Models;

namespace StockMarketServiceDatabase.Services.Queries
{
    public class LiteDBUserQueriesService : IUserQueriesService
    {
        private string _databasePath;
        private const string _queriesCollection = "Queries";

        public LiteDBUserQueriesService(string dbPath)
        {
            _databasePath = dbPath.EndsWith("db") ? dbPath : Path.Combine(dbPath, "UserQueries.db");
        }

        public List<UserQueryModel> GetUserQueries(int userId)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                return queries.Find(q => q.UserId == userId).ToList();
            }
        }

        public int AddOrUpdateQuery(UserQueryModel query)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                queries.EnsureIndex(u => u.Id, true);
                if (query.Id == 0)
                {
                    return queries.Insert(query);
                }
                else
                {
                    queries.Update(query);
                    return query.Id;
                }
            }
        }

        public void DeleteQuery(UserQueryModel query)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                queries.DeleteMany(q => q.Id == query.Id);
            }
        }

        public UserQueryModel GetQuery(int Id)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                return queries.Find(q => q.Id == Id)
                    .FirstOrDefault();
            }
        }
    }
}
