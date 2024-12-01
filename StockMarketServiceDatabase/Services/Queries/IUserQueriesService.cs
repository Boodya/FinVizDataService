using StockMarketServiceDatabase.Models;

namespace StockMarketServiceDatabase.Services.Queries
{
    public interface IUserQueriesService
    {
        UserQueryModel GetQuery(int queryId);
        List<UserQueryModel> GetUserQueries(int userId);
        int AddOrUpdateQuery(UserQueryModel query);
        void DeleteQuery(UserQueryModel query);
    }
}
