using StockMarketAnalyticsService.Models;

namespace StockMarketAnalyticsService.Services
{
    public interface IUserDataService
    {
        UserModel GetUser(string email);
        UserModel GetUserById(int userId);
        void DeleteUser(int userId);
        int AddOrUpdateUser(UserModel user);
        IUserQueriesService QueriesService { get; }  
    }
}
