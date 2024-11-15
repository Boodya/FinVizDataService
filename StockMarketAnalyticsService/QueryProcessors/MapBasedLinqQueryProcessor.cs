using Microsoft.AspNetCore.Mvc.RazorPages;
using StockMarketAnalyticsService.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarketAnalyticsService.QueryProcessors
{
    public class MapBasedLinqQueryProcessor<T> where T : new()
    {
        private readonly string _mapPropName;
        private readonly MapBasedFilterProcessor<T> _filterProcessor;

        public MapBasedLinqQueryProcessor(string mapPropName)
        {
            _mapPropName = mapPropName;
            _filterProcessor = new MapBasedFilterProcessor<T>(mapPropName);
        }

        public List<T> QueryData(List<T> data, LinqProcessorRequestModel request)
        {
            if (!string.IsNullOrEmpty(request.Filter))
            {
                data = _filterProcessor.ApplyFilter(data, request.Filter);
            }

            if (!string.IsNullOrEmpty(request.Sort))
            {
                data = ApplySorting(data, request.Sort);
            }

            if (!string.IsNullOrEmpty(request.Select))
            {
                data = ApplySelection(data, request.Select);
            }

            if (request.Top.HasValue && request.Top > 0 && request.Page.HasValue && request.Page > 0)
            {
                int skip = (request.Page.Value - 1) * request.Top.Value;
                data = data.Skip(skip).Take(request.Top.Value).ToList();
            }
            else if (request.Top.HasValue && request.Top > 0)
            {
                data = data.Take(request.Top.Value).ToList();
            }

            return data;
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
                                if (double.TryParse(matchingEntry.Value.Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue))
                                {
                                    return numericValue;
                                }

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

        private List<T> ApplySelection(List<T> data, string select)
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
