using StockMarketAnalyticsService.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarketAnalyticsService.QueryProcessors
{
    public class MapBasedLinqQueryProcessor<T> where T : new()
    {
        private readonly string _mapPropName;

        public MapBasedLinqQueryProcessor(string mapPropName)
        {
            _mapPropName = mapPropName;
        }

        public List<T> QueryData(List<T> data, LinqProcessorRequestModel request)
        {
            if (!string.IsNullOrEmpty(request.Filter))
            {
                data = ApplyFilter(data, request.Filter);
            }

            if (!string.IsNullOrEmpty(request.Sort))
            {
                data = ApplySorting(data, request.Sort);
            }

            if (!string.IsNullOrEmpty(request.Select))
            {
                data = ApplyProjection(data, request.Select);
            }

            if (request.Top.HasValue && request.Top > 0)
            {
                data = data.Take(request.Top.Value).ToList();
            }

            return data;
        }

        private List<T> ApplyFilter(List<T> data, string filter)
        {
            // Split by 'AND'/'OR', handling cases where there is no logical operator as well
            var conditions = Regex.Split(filter, @"\s+(AND|OR)\s+", RegexOptions.IgnoreCase)
                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                  .ToList();

            return data.Where(item =>
            {
                var dictPropInfo = typeof(T).GetProperty(_mapPropName);
                if (dictPropInfo == null || dictPropInfo.PropertyType != typeof(Dictionary<string, string>))
                    return false;

                var itemProperties = (Dictionary<string, string>)dictPropInfo.GetValue(item);
                if (itemProperties == null)
                    return false;

                // Evaluate the first condition
                bool result = EvaluateCondition(itemProperties, conditions[0]);

                // Process any additional conditions with 'AND'/'OR' operators
                for (int i = 1; i < conditions.Count; i += 2)
                {
                    var logicalOperator = conditions[i].Trim().ToUpper();
                    var nextCondition = conditions[i + 1];

                    var nextConditionResult = EvaluateCondition(itemProperties, nextCondition);

                    if (logicalOperator == "AND")
                    {
                        result = result && nextConditionResult;
                    }
                    else if (logicalOperator == "OR")
                    {
                        result = result || nextConditionResult;
                    }
                }

                return result;

            }).ToList();
        }

        private bool EvaluateCondition(Dictionary<string, string> itemProperties, string condition)
        {
            // Define possible operators in order of descending length
            string[] operators = { "!=", ">=", "<=", ">", "<", "=" };
            string property = null;
            string @operator = null;
            string value = null;

            // Find the operator in the condition
            foreach (var op in operators)
            {
                var opIndex = condition.IndexOf(op, StringComparison.Ordinal);
                if (opIndex > -1)
                {
                    property = condition.Substring(0, opIndex).Trim().ToLower();
                    @operator = op;
                    value = condition.Substring(opIndex + op.Length).Trim();
                    break;
                }
            }

            // If no operator was found, default to "=" for simple conditions like "Ticker=MSFT"
            if (@operator == null)
            {
                @operator = "=";
                property = condition.Split('=')[0].Trim().ToLower();
                value = condition.Split('=')[1].Trim();
            }

            // Perform a case-insensitive lookup in itemProperties
            var dictEntry = itemProperties.FirstOrDefault(kv => kv.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(dictEntry.Key) ||
                string.IsNullOrEmpty(dictEntry.Value))
                return false;

            var dictValue = dictEntry.Value
                .ToLower().Replace("%", ""); // Normalize case for comparison

            // Attempt numeric comparison if both values can be parsed as numbers
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue) &&
                double.TryParse(dictValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var dictNumericValue))
            {
                return @operator switch
                {
                    "=" => dictNumericValue == numericValue,
                    "!=" => dictNumericValue != numericValue,
                    ">" => dictNumericValue > numericValue,
                    "<" => dictNumericValue < numericValue,
                    ">=" => dictNumericValue >= numericValue,
                    "<=" => dictNumericValue <= numericValue,
                    _ => false,
                };
            }

            // Fallback to case-insensitive string comparison for non-numeric values
            value = value.ToLower(); // Normalize case for string comparison
            return @operator switch
            {
                "=" => dictValue == value,
                "!=" => dictValue != value,
                ">" => string.Compare(dictValue, value, StringComparison.OrdinalIgnoreCase) > 0,
                "<" => string.Compare(dictValue, value, StringComparison.OrdinalIgnoreCase) < 0,
                ">=" => string.Compare(dictValue, value, StringComparison.OrdinalIgnoreCase) >= 0,
                "<=" => string.Compare(dictValue, value, StringComparison.OrdinalIgnoreCase) <= 0,
                _ => false,
            };
        }

        private List<T> ApplySorting(List<T> data, string sort)
        {
            var sortExpressions = sort.Split(',');
            IOrderedEnumerable<T> orderedData = null;

            for (int i = 0; i < sortExpressions.Length; i++)
            {
                var match = Regex.Match(sortExpressions[i].Trim(), @"^(.*?)(\s+(asc|desc))?$", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    throw new ArgumentException($"Invalid sort expression: {sortExpressions[i]}");
                }

                var property = match.Groups[1].Value.Trim();
                var descending = match.Groups[3].Success && match.Groups[3].Value.ToLower() == "desc";

                Func<T, object> keySelector = item =>
                {
                    var dictPropInfo = typeof(T).GetProperty(_mapPropName);
                    if (dictPropInfo != null && dictPropInfo.PropertyType == typeof(Dictionary<string, string>))
                    {
                        var itemProperties = (Dictionary<string, string>)dictPropInfo.GetValue(item);
                        if (itemProperties != null)
                        {
                            var matchingEntry = itemProperties
                                .FirstOrDefault(kv => kv.Key.Equals(property, StringComparison.OrdinalIgnoreCase));

                            if (matchingEntry.Value != null)
                            {
                                // Remove '%' and try to parse the value as a double
                                if (double.TryParse(matchingEntry.Value.Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue))
                                {
                                    return numericValue;
                                }

                                // If parsing fails, return the original string for lexicographical comparison
                                return matchingEntry.Value;
                            }
                        }
                    }

                    return null;
                };

                orderedData = i == 0
                    ? (descending ? data.OrderByDescending(keySelector) : data.OrderBy(keySelector))
                    : (descending ? orderedData.ThenByDescending(keySelector) : orderedData.ThenBy(keySelector));
            }

            return orderedData?.ToList() ?? data;
        }

        private List<T> ApplyProjection(List<T> data, string select)
        {
            var selectedProperties = select.Split(',').Select(prop => prop.Trim().ToLower()).ToList();
            var projectedData = new List<T>();

            foreach (var item in data)
            {
                var projectedItem = new T();
                foreach (var property in typeof(T).GetProperties().Where(p => p.Name != _mapPropName))
                {
                    property.SetValue(projectedItem, property.GetValue(item));
                }

                var dictPropInfo = typeof(T).GetProperty(_mapPropName);
                if (dictPropInfo != null && dictPropInfo.PropertyType == typeof(Dictionary<string, string>))
                {
                    var itemProperties = (Dictionary<string, string>)dictPropInfo.GetValue(item);
                    var projectedDict = new Dictionary<string, string>();

                    if (itemProperties != null)
                    {
                        foreach (var key in selectedProperties)
                        {
                            var matchingEntry = itemProperties
                                .FirstOrDefault(kv => kv.Key.ToLower() == key);
                            if (matchingEntry.Key != null)
                            {
                                projectedDict[matchingEntry.Key] = matchingEntry.Value;
                            }
                        }
                    }

                    dictPropInfo.SetValue(projectedItem, projectedDict);
                }

                projectedData.Add(projectedItem);
            }

            return projectedData;
        }
    }
}
