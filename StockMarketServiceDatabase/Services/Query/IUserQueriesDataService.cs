using StockMarketServiceDatabase.Models.Query;

namespace StockMarketServiceDatabase.Services.Query
{
    public interface IUserQueriesDataService
    {
        UserQueryModel GetQuery(int Id);
        List<UserQueryModel> GetUserQueries(int userId);
        int AddOrUpdateQuery(UserQueryModel query);
        void DeleteQuery(UserQueryModel query);
    }
}
