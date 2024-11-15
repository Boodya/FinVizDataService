using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarketAnalyticsService.QueryProcessors
{
    public class MapBasedFilterProcessor<T>
    {
        private readonly string _mapPropName;
        public MapBasedFilterProcessor(string mapPropName)
        {
            _mapPropName = mapPropName;
        }

        public List<T> ApplyFilter(List<T> data, string filter)
        {
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

                bool result = EvaluateCondition(itemProperties, conditions[0]);

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
            string[] operators = { "!=", ">=", "<=", ">", "<", "=" };
            string leftProperty = null;
            string @operator = null;
            string rightPropertyOrValue = null;

            foreach (var op in operators)
            {
                var opIndex = condition.IndexOf(op, StringComparison.Ordinal);
                if (opIndex > -1)
                {
                    leftProperty = condition.Substring(0, opIndex).Trim();
                    @operator = op;
                    rightPropertyOrValue = condition.Substring(opIndex + op.Length).Trim();
                    break;
                }
            }

            if (@operator == null)
            {
                throw new ArgumentException($"Invalid condition: {condition}");
            }

            var leftValue = itemProperties.FirstOrDefault(kv => kv.Key.Equals(leftProperty, StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrEmpty(leftValue))
            {
                return false;
            }
            leftValue = leftValue.ToLower().Replace("%", "").Trim();

            var rightValue = itemProperties.FirstOrDefault(kv => kv.Key.Equals(rightPropertyOrValue, StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrEmpty(rightValue))
            {
                rightValue = rightPropertyOrValue;
            }
            rightValue = rightValue.ToLower().Replace("%", "").Trim();

            if (double.TryParse(leftValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var leftNumericValue) &&
                double.TryParse(rightValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightNumericValue))
            {
                return @operator switch
                {
                    "=" => leftNumericValue == rightNumericValue,
                    "!=" => leftNumericValue != rightNumericValue,
                    ">" => leftNumericValue > rightNumericValue,
                    "<" => leftNumericValue < rightNumericValue,
                    ">=" => leftNumericValue >= rightNumericValue,
                    "<=" => leftNumericValue <= rightNumericValue,
                    _ => false,
                };
            }

            return @operator switch
            {
                "=" => string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase),
                "!=" => !string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase),
                ">" => string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase) > 0,
                "<" => string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase) < 0,
                ">=" => string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase) >= 0,
                "<=" => string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase) <= 0,
                _ => false,
            };
        }
    }
}
