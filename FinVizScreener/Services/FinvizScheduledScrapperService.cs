using FinVizDataService.Models;
using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Scrappers;

namespace FinVizScreener.Services
{
    public class FinvizScheduledScrapperService
    {
        public List<FinVizDataItem> Data { get; private set; }
        private IScrapper _scrapper;
        private FinVizDataServiceConfigModel _cfg;
        private CancellationToken _scrapperCancelToken;
        private Dictionary<Guid,Action<FinVizDataItem>> _subscribers;
        private readonly object _subscriptionLock = new object();
        private TimeSpan _broadcastingDelay;
        private DateTime _lastBroadCastDate;
        private List<FinVizDataItem> _dataSubscribersQueue;

        public FinvizScheduledScrapperService(FinVizDataServiceConfigModel cfg)
        {
            _subscribers = new();
            _scrapper = new PaginatedFullScrapper();
            _cfg = cfg;
            Data = _cfg.Db
                .GetLatestData()
                .ToList();
            _scrapperCancelToken = new CancellationToken();
            _dataSubscribersQueue = new();
            _broadcastingDelay = TimeSpan.FromMilliseconds(50);
            StartPeriodicScrapingAsync(_scrapperCancelToken);
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

        private async Task StartPeriodicScrapingAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndSaveDataAsync();
                    await Task.Delay(_cfg.DataFetchPeriod, cancellationToken);
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
            _cfg.Db.SaveData(dataItems);
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
