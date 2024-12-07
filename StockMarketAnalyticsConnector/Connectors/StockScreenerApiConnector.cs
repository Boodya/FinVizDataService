using StockMarketServiceDatabase.Models.FinViz;
using System.Net.Http.Json;

namespace StockMarketAnalyticsConnector.Connectors
{
    public class StockScreenerApiConnector
    {
        private readonly HttpClient _httpClient;

        public StockScreenerApiConnector(string enpointUrl)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(enpointUrl);
        }

        public async Task<List<string>> GetTickerListAsync(string searchTerm = "")
        {
            var response = await _httpClient.GetAsync($"StockScreenerApi/GetTickers?searchTerm={Uri.EscapeDataString(searchTerm)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<string>>();
        }

        public async Task<FinVizDataItem> GetInstrumentDataAsync(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("Ticker cannot be empty.", nameof(ticker));

            var response = await _httpClient.GetAsync($"StockScreenerApi/FindByExactTicker?ticker={Uri.EscapeDataString(ticker)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FinVizDataItem>();
        }

        public async Task<List<FinVizDataItem>> SearchByTickerAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty.", nameof(searchTerm));

            var response = await _httpClient.GetAsync($"StockScreenerApi/SearchByTicker?searchTerm={Uri.EscapeDataString(searchTerm)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FinVizDataItem>>();
        }

        public async Task<List<FinVizDataItem>> FetchPaginatedDataAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                throw new ArgumentException("Page and pageSize must be greater than zero.");

            var response = await _httpClient.GetAsync($"StockScreenerApi/FetchPaginatedData?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FinVizDataItem>>();
        }

        public async Task<List<FinVizDataItem>> FetchAllLatestDataAsync()
        {
            var response = await _httpClient.PostAsync("StockScreenerApi/FetchAllLatestData", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FinVizDataItem>>();
        }
    }
}
