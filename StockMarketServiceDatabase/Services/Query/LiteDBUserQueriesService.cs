using LiteDB;
using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using System.Linq.Expressions;

namespace StockMarketServiceDatabase.Services.Query
{
    public class LiteDBUserQueriesService : IUserQueriesDataService
    {
        private string _databasePath;
        private const string _queriesCollection = "Queries";
        private const string _queryCalculations = "QueryCalculations";

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

        public List<UserQueryModel> GetQuery(
            Expression<Func<UserQueryModel, bool>> condition)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                return queries.Find(condition).ToList();
            }
        }

        public void IterateQueries(Action<IEnumerable<UserQueryModel>> batchProcessing, int batchSize=100)
        {
            if (batchProcessing == null)
            {
                throw new ArgumentNullException(nameof(batchProcessing));
            }

            if (batchSize <= 0)
            {
                throw new ArgumentException("Batch size must be greater than zero.", nameof(batchSize));
            }

            using (var db = new LiteDatabase(_databasePath))
            {
                var queries = db.GetCollection<UserQueryModel>(_queriesCollection);
                var skip = 0;
                IEnumerable<UserQueryModel> batch;

                do
                {
                    batch = queries.FindAll()
                        .Skip(skip)
                        .Take(batchSize)
                        .ToList();

                    if (batch.Any())
                    {
                        batchProcessing(batch);
                    }

                    skip += batchSize;
                }
                while (batch.Count() == batchSize);
            }
        }

        public FilterCalculationResultModel? GetFilterCalculationResult(int queryId)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var calcDb = db.GetCollection<FilterCalculationResultModel>(_queryCalculations);
                return calcDb.Find(q => q.QueryId == queryId)
                    .FirstOrDefault();
            }
        }

        public void AddOrUpdateQueryCalculation(FilterCalculationResultModel calculation)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var calcDb = db.GetCollection<FilterCalculationResultModel>(_queryCalculations);
                calcDb.EnsureIndex(u => u.QueryId, true);
                calcDb.Upsert(calculation);
                var queriesDb = db.GetCollection<UserQueryModel>(_queriesCollection);
                var query = queriesDb.FindById(calculation.QueryId);
                query.RevisionNumber = calculation.LastDataRevisionNum;
                queriesDb.Update(query);
            }
        }

        public List<FilterCalculationResultModel> GetQueryCalculations(
            Expression<Func<FilterCalculationResultModel, bool>> condition)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var calcDb = db.GetCollection<FilterCalculationResultModel>(_queryCalculations);
                return calcDb.Find(condition).ToList();
            }
        }
    }
}
