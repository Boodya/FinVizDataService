using FinVizDataService.Models;
using LiteDB;

namespace FinVizScreener.DB
{
    public class LocalLiteDBAdapter : IFinvizDBAdapter
    {
        private readonly string _dbPath;
        private const string CollectionName = "FinVizData";

        public LocalLiteDBAdapter(string dbPath)
        {
            _dbPath = dbPath;
        }

        public IEnumerable<FinVizDataItem> GetLatestData()
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                var collection = db.GetCollection<FinVizDataItem>(CollectionName);
                var allItems = collection.FindAll().ToList();

                var latestItems = allItems
                    .GroupBy(i => i.Ticker)
                    .Select(group => group
                        .OrderByDescending(item => item.Version)
                        .FirstOrDefault())
                    .Where(item => item != null)
                    .ToList();

                return latestItems;
            }
        }

        public int SaveData(IEnumerable<FinVizDataItem> data)
        {
            var validatedData = ValidatePropsUpdated(data);
            if (validatedData.Count == 0)
                return 0;
            using (var db = new LiteDatabase(_dbPath))
            {
                var collection = db.GetCollection<FinVizDataItem>(CollectionName);
                collection.EnsureIndex(item => item.Id, unique: true);
                collection.EnsureIndex(item => item.Version);

                var currentMaxVersion = collection.Query()
                    .OrderByDescending(item => item.Version)
                    .Select(item => item.Version)
                    .FirstOrDefault();
                var currentMaxId = collection.Query()
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();

                int newId = currentMaxId + 1;
                int newVersion = currentMaxVersion + 1;
                foreach (var item in validatedData)
                {
                    item.Id = newId;
                    item.Version = newVersion;
                    newId++;
                }

                collection.InsertBulk(validatedData);
            }
            return validatedData.Count;
        }

        private List<FinVizDataItem> ValidatePropsUpdated(IEnumerable<FinVizDataItem> data)
        {
            var result = new List<FinVizDataItem>();
            using (var db = new LiteDatabase(_dbPath))
            {
                var collection = db.GetCollection<FinVizDataItem>(CollectionName);
                var latestDbItems = collection.Query()
                    .OrderByDescending(i => i.Version)
                    .ToEnumerable()
                    .GroupBy(i => i.Ticker)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var item in data)
                {
                    if (latestDbItems.TryGetValue(item.Ticker, out var dbLastItem))
                    {
                        if (IsDifferent(dbLastItem, item))
                        {
                            result.Add(item);
                        }
                    }
                    else
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        private bool IsDifferent(FinVizDataItem? left, FinVizDataItem? right)
        {
            if (left == null || right == null)
                return true;
            if(left.ItemProperties.Keys.Count != right.ItemProperties.Keys.Count)
                return true;
            foreach (var leftKey in left.ItemProperties.Keys)
            {
                if (!right.ItemProperties.ContainsKey(leftKey))
                    return true;
                if (left.ItemProperties[leftKey] != right.ItemProperties[leftKey])
                    return true;
            }
            return false;
        }
    }
}
