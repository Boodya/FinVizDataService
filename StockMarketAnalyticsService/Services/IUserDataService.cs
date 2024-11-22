using StockMarketAnalyticsService.Models;

namespace StockMarketAnalyticsService.Services
{
    public interface IUserDataService
    {
        UserQueryModel GetUser(string email);
        void SetUser(UserQueryModel user);
        List<LinqProcessorRequestModel> GetUserQueries(string email);
        void SaveQuery(string email, LinqProcessorRequestModel query);
        void DeleteQuery(string email, LinqProcessorRequestModel query);
    }

}
