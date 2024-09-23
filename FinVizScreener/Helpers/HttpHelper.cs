namespace FinVizScreener.Helpers
{
    internal static class HttpHelper
    {
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static TimeSpan _requestsDencity = new(0,0,1);
        private static TimeSpan _browserRestartPeriod = new(0, 5, 0);
        private static readonly object _lock = new object();
        private static HttpClient? _browserClient = null;
        public static HttpClient GetBrowserRequestHandler()
        {
            var handler = new HttpClientHandler() { UseCookies = true };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            return client;
        }

        public static async Task<HttpResponseMessage> GetAsync(string url)
        {
            WaitRequestsTimeout();
            _lastRequestTime = DateTime.Now;
            var responce = await GetBrowserClient()
                .GetAsync(url);
            return responce;
        }

        private static HttpClient GetBrowserClient()
        {
            var tp = DateTime.Now - _lastRequestTime;
            if (tp > _browserRestartPeriod)
            {
                _browserClient?.Dispose();
                _browserClient = null;
            }    
            if (_browserClient == null)
                _browserClient = GetBrowserRequestHandler();
            return _browserClient;
        }

        private static void WaitRequestsTimeout()
        {
            var tp = DateTime.Now - _lastRequestTime;
            if (tp <= _requestsDencity)
                Thread.Sleep(_requestsDencity - tp);
        }
    }
}
