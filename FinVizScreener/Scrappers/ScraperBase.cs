using FinVizScreener.Helpers;

namespace FinVizScreener.Scrapers
{
    public abstract class ScraperBase
    {
        protected async Task<string> GetPageContentAsync(string url)
        {
            var response = await HttpHelper.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        protected decimal? ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return null;
        }

        protected int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
