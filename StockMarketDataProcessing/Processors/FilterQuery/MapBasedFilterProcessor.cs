using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarketDataProcessing.Processors.FilterQuery
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
            string[] operators = { "!=", ">=", "<=", ">", "<", "=", "contains" };
            string leftExpression = null;
            string @operator = null;
            string rightExpression = null;

            foreach (var op in operators)
            {
                var opIndex = condition.IndexOf(op, StringComparison.OrdinalIgnoreCase);
                if (opIndex > -1)
                {
                    leftExpression = condition.Substring(0, opIndex).Trim();
                    @operator = op;
                    rightExpression = condition.Substring(opIndex + op.Length).Trim();
                    break;
                }
            }

            if (@operator == null)
            {
                throw new ArgumentException($"Invalid condition: {condition}");
            }

            var leftValue = EvaluateExpression(itemProperties, leftExpression);
            var rightValue = EvaluateExpression(itemProperties, rightExpression);

            leftValue = leftValue.ToLowerInvariant();
            rightValue = rightValue.ToLowerInvariant();

            if (@operator.Equals("contains", StringComparison.OrdinalIgnoreCase))
            {
                return leftValue.Contains(rightValue, StringComparison.OrdinalIgnoreCase);
            }

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

            if (DateTime.TryParse(leftValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var leftDateValue) &&
                DateTime.TryParse(rightValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var rightDateValue))
            {
                return @operator switch
                {
                    "=" => leftDateValue == rightDateValue,
                    "!=" => leftDateValue != rightDateValue,
                    ">" => leftDateValue > rightDateValue,
                    "<" => leftDateValue < rightDateValue,
                    ">=" => leftDateValue >= rightDateValue,
                    "<=" => leftDateValue <= rightDateValue,
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

        private string EvaluateExpression(Dictionary<string, string> itemProperties, string expression)
        {
            var propertyMatch = Regex.Match(expression, @"^\[(.*?)\]$");
            if (propertyMatch.Success)
            {
                var propertyName = propertyMatch.Groups[1].Value;
                return itemProperties.FirstOrDefault(kv => kv.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;
            }

            var arithmeticMatch = Regex.Match(expression, @"^\[(.*?)\]\s*([-+*/])\s*\[(.*?)\]$");
            if (arithmeticMatch.Success)
            {
                var leftProperty = arithmeticMatch.Groups[1].Value;
                var @operator = arithmeticMatch.Groups[2].Value;
                var rightProperty = arithmeticMatch.Groups[3].Value;

                var leftValue = double.TryParse(
                    itemProperties.FirstOrDefault(kv => kv.Key.Equals(leftProperty, StringComparison.OrdinalIgnoreCase)).Value ?? "0",
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var leftNumericValue)
                    ? leftNumericValue
                    : 0;

                var rightValue = double.TryParse(
                    itemProperties.FirstOrDefault(kv => kv.Key.Equals(rightProperty, StringComparison.OrdinalIgnoreCase)).Value ?? "0",
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var rightNumericValue)
                    ? rightNumericValue
                    : 0;

                return @operator switch
                {
                    "+" => (leftNumericValue + rightNumericValue).ToString(CultureInfo.InvariantCulture),
                    "-" => (leftNumericValue - rightNumericValue).ToString(CultureInfo.InvariantCulture),
                    "*" => (leftNumericValue * rightNumericValue).ToString(CultureInfo.InvariantCulture),
                    "/" => (rightNumericValue != 0 ? (leftNumericValue / rightNumericValue) : 0).ToString(CultureInfo.InvariantCulture),
                    _ => "0",
                };
            }

            return expression;
        }
    }
}
