using FinVizDataService.Models;
using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Scrappers;

namespace FinVizScreener.Services
{
    public class FinvizScheduledScrapperService
    {
        public FinVizDataPack Data { get; private set; }
        private IScrapper _scrapper;
        private FinVizDataServiceConfigModel _cfg;
        private CancellationToken _scrapperCancelToken;
        private Dictionary<Guid,Action<FinVizDataPack>> _subscribers;
        private readonly object _subscribersLock = new object();

        public FinvizScheduledScrapperService(FinVizDataServiceConfigModel cfg)
        {
            _subscribers = new();
            _scrapper = new PaginatedFullScrapper();
            _cfg = cfg;
            Data = _cfg.Db.GetLatestData();
            _scrapperCancelToken = new CancellationToken();
            StartPeriodicScrapingAsync(_scrapperCancelToken);
        }

        public Guid SubscribeOnDataUpdated(Action<FinVizDataPack> onDataUpdated)
        {
            lock (_subscribersLock)
            {
                var id = Guid.NewGuid();
                _subscribers.Add(id, onDataUpdated);
                return id;
            }
        }

        public void Unsibscribe(Guid subscriptionId)
        {
            lock (_subscribersLock)
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
                dataItems.Add(item);
            var newData = new FinVizDataPack
            {
                FetchDate = DateTime.Now,
                Items = dataItems
            };
            _cfg.Db.SaveData(newData);
            Data = newData;
            HandleSubscribers(newData);
            
        }

        private void HandleSubscribers(FinVizDataPack data)
        {
            lock (_subscribersLock)
            {
                foreach (var subscriber in _subscribers.Values)
                    Task.Run(() => subscriber(data));
            }
        }
    }
}
