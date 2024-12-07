using StockMarketServiceDatabase.Models.User;
using StockMarketServiceDatabase.Services.Query;

namespace StockMarketServiceDatabase.Services.User
{
    public interface IUserDataService
    {
        UserModel? GetUser(string email);
        UserModel? GetUserById(int userId);
        void DeleteUser(int userId);
        int AddOrUpdateUser(UserModel user);
        IUserQueriesDataService QueriesService { get; }
    }
}
