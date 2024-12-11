using System.Diagnostics;
using System.Globalization;

namespace StockMarketDataProcessing
{
    internal static class Helpers
    {
        public static decimal ToDecimal(string value)
        {
            decimal decVal = 0;
            if (!string.IsNullOrEmpty(value))
                decimal.TryParse(value, NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out decVal);
            return decVal;
        }

        public static decimal Percent(decimal first, decimal second)
        {
            return first / second * 100m;
        }
    }
}
