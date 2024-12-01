using StockMarketServiceDatabase.Models;
using StockMarketServiceDatabase.Services.Queries;

namespace StockMarketServiceDatabase.Services
{
    public interface IUserDataService
    {
        UserModel? GetUser(string email);
        UserModel? GetUserById(int userId);
        void DeleteUser(int userId);
        int AddOrUpdateUser(UserModel user);
        IUserQueriesService QueriesService { get; }  
    }
}
