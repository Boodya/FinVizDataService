using FinVizDataService.Models;
using FinVizScreener.DB;
using FinVizScreener.Helpers;
using FinVizScreener.Models;
using FinVizScreener.Scrappers;
using Microsoft.Extensions.Logging;

namespace FinVizScreener.Services
{
    public class FinVizScrapperService
    {
        public List<FinVizDataItem> Data { get; private set; }
        private IScrapper _scrapper;
        private FinVizDataServiceConfigModel _cfg;
        private CancellationToken _scrapperCancelToken;
        private Dictionary<Guid,Action<FinVizDataItem>> _subscribers;
        private readonly object _subscriptionLock = new object();
        private IFinvizDBAdapter _db;
        private readonly ILogger<FinVizScrapperService>? _logger;

        public FinVizScrapperService(FinVizDataServiceConfigModel cfg, ILogger<FinVizScrapperService>? logger=null)
        {
            _logger = logger;
            _subscribers = new();
            _scrapper = new PaginatedFullScrapper();
            _cfg = cfg;
            _db = DBAdapterFactory.Resolve(_cfg.DatabaseType, _cfg.DatabaseConnectionString);
            Data = _db
                .GetLatestData()
                .ToList();
            _scrapperCancelToken = new CancellationToken();
            _ = StartPeriodicScrapingAsync(_scrapperCancelToken);
        }

        public Guid Subscribe(Action<FinVizDataItem> onDataUpdated)
        {
            lock (_subscriptionLock)
            {
                var id = Guid.NewGuid();
                _subscribers.Add(id, onDataUpdated);
                return id;
            }
        }

        public void Unsibscribe(Guid subscriptionId)
        {
            lock (_subscriptionLock)
            {
                if (_subscribers.ContainsKey(subscriptionId))
                    _subscribers.Remove(subscriptionId);
            }  
        }

        private async Task StartPeriodicScrapingAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await ScheduledExecutor.RunScheduledTask(_cfg.ExecutionTime, ct, async () =>
                    {
                        _logger?.Log(LogLevel.Information, $"FinVizScrapperService: " +
                            $"performing data fetch");
                        await FetchAndSaveDataAsync();
                    }, _logger);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async Task FetchAndSaveDataAsync()
        {
            var dataItems = new List<FinVizDataItem>();
            await foreach (var item in _scrapper.ScrapeDataTableAsync(_cfg.EndpointUrl))
            {
                dataItems.Add(item);
                HandleSubscribers(item);
            }
            var tSaved = _db.SaveData(dataItems);
            _logger?.Log(LogLevel.Information, $"FinVizScrapperService: " +
                $"data downloading completed. {tSaved} notes saved to DB");
            Data = dataItems;
        }

        private void HandleSubscribers(FinVizDataItem data)
        {
            lock (_subscriptionLock)
            {
                foreach (var subscriber in _subscribers.Values)
                    Task.Run(() => subscriber(data));
            }
        }
    }
}
