using System.Globalization;

namespace FinVizScreener.Helpers
{
    internal static class PropsValidator
    {
        public static string ValidatePropValue(string value)
        {
            if (ValidateEmpty(value))
                return "";

            var validated = GetMultipliedValue(value)?.ToString("f2", CultureInfo.InvariantCulture);
            return validated ?? value; 
        }

        private static double? GetMultipliedValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            char last = value.Last();
            if (last != 'K' && last != 'M' && last != 'B')
                return null;

            try
            {
                var str = new string(value.Take(value.Length - 1).ToArray());
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double numValue))
                {
                    return last switch
                    {
                        'K' => numValue * 1_000,
                        'M' => numValue * 1_000_000,
                        'B' => numValue * 1_000_000_000,
                        _ => null
                    };
                }
            }
            catch(Exception ex)
            {
                
            }
            return null;
        }

        private static bool ValidateEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;
            return value == "-";
        }
    }
}
