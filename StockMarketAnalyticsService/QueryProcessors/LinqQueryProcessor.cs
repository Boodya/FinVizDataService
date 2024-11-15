using StockMarketAnalyticsService.Models;
using System.Reflection;

namespace StockMarketAnalyticsService.QueryProcessors
{
    public class LinqQueryProcessor<T>
    {
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
            var filterParts = filter.Split('=');
            var property = filterParts[0].Trim();
            var value = filterParts[1].Trim();

            var propInfo = typeof(T).GetProperty(property);
            if (propInfo == null) return data;

            return data.Where(item => propInfo.GetValue(item)?.ToString() == value).ToList();
        }

        private List<T> ApplySorting(List<T> data, string sort)
        {
            var sortExpressions = sort.Split(',');
            IOrderedEnumerable<T> orderedData = null;

            for (int i = 0; i < sortExpressions.Length; i++)
            {
                var sortParts = sortExpressions[i].Trim().Split(' ');
                var property = sortParts[0].Trim();
                var descending = sortParts.Length > 1 && sortParts[1].ToLower() == "desc";

                var propInfo = typeof(T).GetProperty(property);
                if (propInfo == null) continue;

                if (i == 0)
                {
                    orderedData = descending
                        ? data.OrderByDescending(item => propInfo.GetValue(item))
                        : data.OrderBy(item => propInfo.GetValue(item));
                }
                else
                {
                    orderedData = descending
                        ? orderedData.ThenByDescending(item => propInfo.GetValue(item))
                        : orderedData.ThenBy(item => propInfo.GetValue(item));
                }
            }

            return orderedData?.ToList() ?? data;
        }

        private List<T> ApplyProjection(List<T> data, string select)
        {
            var selectedProperties = select.Split(',').Select(prop => prop.Trim()).ToList();
            var projectedData = new List<T>();

            foreach (var item in data)
            {
                var projectedItem = Activator.CreateInstance<T>();

                foreach (var propertyName in selectedProperties)
                {
                    var propertyInfo = typeof(T).GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        var value = propertyInfo.GetValue(item);
                        propertyInfo.SetValue(projectedItem, value);
                    }
                }

                projectedData.Add(projectedItem);
            }

            return projectedData;
        }
    }
}
