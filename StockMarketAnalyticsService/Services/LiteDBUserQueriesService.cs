using LiteDB;
using StockMarketAnalyticsService.Models;

namespace StockMarketAnalyticsService.Services
{
    public class LiteDBUserQueriesService : IUserQueriesService
    {
        private string _databasePath;
        private const string _queriesCollection = "Queries";

        public LiteDBUserQueriesService(string dbPath)
        {
            _databasePath = dbPath;
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

                queries.EnsureIndex(u => u.QueryId, true);

                if (query.QueryId == 0)
                {
                    return queries.Insert(query);
                }
                else
                {
                    queries.Update(query);
                    return query.QueryId;
                }
            }
        }

        public void DeleteQuery(UserQueryModel query)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                queries.DeleteMany(q => q.QueryId == query.QueryId);
            }
        }

        public UserQueryModel GetQuery(int queryId)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                return queries.Find(q => q.QueryId == queryId)
                    .FirstOrDefault();
            }
        }
    }
}
