using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using System.Linq.Expressions;

namespace StockMarketServiceDatabase.Services.Query
{
    public interface IUserQueriesDataService
    {
        public void IterateQueries(Action<IEnumerable<UserQueryModel>> batchProcessing, int batchSize=100);
        public UserQueryModel GetQuery(int Id);
        public List<UserQueryModel> GetQuery(
            Expression<Func<UserQueryModel, bool>> condition);
        public List<UserQueryModel> GetUserQueries(int userId);
        public int AddOrUpdateQuery(UserQueryModel query);
        public void DeleteQuery(UserQueryModel query);
        //Query Calculations
        public List<FilterCalculationResultModel> GetQueryCalculations(
            Expression<Func<FilterCalculationResultModel, bool>>? condition=null);
        public FilterCalculationResultModel? GetFilterCalculationResult(int queryId);
        public void AddOrUpdateQueryCalculation(FilterCalculationResultModel calculations);
    }
}
