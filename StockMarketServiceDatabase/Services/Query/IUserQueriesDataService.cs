using StockMarketServiceDatabase.Models.Query;
using System.Runtime.CompilerServices;

namespace StockMarketServiceDatabase.Services.Query
{
    public interface IUserQueriesDataService
    {
        void IterateQueries(Action<IEnumerable<UserQueryModel>> batchProcessing, int batchSize=100);
        UserQueryModel GetQuery(int Id);
        List<UserQueryModel> GetUserQueries(int userId);
        int AddOrUpdateQuery(UserQueryModel query);
        void DeleteQuery(UserQueryModel query);
    }
}
