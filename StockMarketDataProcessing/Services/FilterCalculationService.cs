using FinVizScreener.Helpers;
using Microsoft.Extensions.Logging;
using StockMarketDataProcessing.Processors.FilterResults;
using StockMarketServiceDatabase.Models.Processing;
using StockMarketServiceDatabase.Models.Query;
using StockMarketServiceDatabase.Models.User;
using StockMarketServiceDatabase.Services.Query;

namespace StockMarketDataProcessing.Services
{
    public class FilterCalculationService
    {
        private UserDataServiceConfigModel _cfg;
        private IFilterResultsProcessor _processor;
        private IUserQueriesDataService _queries;
        private CancellationToken _calculationCancellation;
        private readonly ILogger<FilterCalculationService>? _logger;

        public FilterCalculationService(IFilterResultsProcessor processor, 
            IUserQueriesDataService queries,
            ILogger<FilterCalculationService>? logger,
            UserDataServiceConfigModel cfg)
        {
            _processor = processor;
            _queries = queries;
            _cfg = cfg;
            _logger = logger;
            _calculationCancellation = new();
            _ = ScheduledExecutor.ScheduleTaskExecution<FilterCalculationService>(cfg.QueryCalculationTime,
                _calculationCancellation,
                CalculateAllQueriesAsync,
                cfg.QueryCalculationOnStart, _logger);
        }

        public FilterCalculationResultModel Calculate(int queryId)
        {
            try
            {
                return _processor.Calculate(queryId);
            }
            catch(Exception ex)
            {
                return new FilterCalculationResultModel()
                {
                    CalculationDate = DateTime.Now.ToUniversalTime(),
                    CalculationError = ex.Message
                };
            }
        }

        public FilterCalculationResultModel Calculate(UserQueryModel query)
        {
            try
            {
                return _processor.Calculate(query);
            }
            catch (Exception ex)
            {
                return new FilterCalculationResultModel()
                {
                    CalculationDate = DateTime.Now.ToUniversalTime(),
                    CalculationError = ex.Message
                };
            }
        }

        private async Task CalculateAllQueriesAsync()
        {
            _logger?.Log(LogLevel.Information, $"FilterCalculationService: " +
                        $"performing queries calculations");

            var queries = _processor.RecalculateAllQueries();
            var totallyCalculated = queries?.Count;
            //queries?.ForEach(_queries.AddOrUpdateQueryCalculation);

            _logger?.Log(LogLevel.Information, $"FilterCalculationService: " +
                $"queries calculation completed. {totallyCalculated} notes saved to DB");
        }
    }
}
