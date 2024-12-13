using StockMarketServiceDatabase.Models.FinViz;
using LiteDB;
using System.Text.RegularExpressions;

namespace StockMarketServiceDatabase.Services.FinViz
{
    public class LocalLiteDBSeparateFilesAdapter : IFinvizDBAdapter
    {
        private readonly string _dbPath;
        private const string _collectionName = "FinVizData";

        public LocalLiteDBSeparateFilesAdapter(string dbPath)
        {
            _dbPath = dbPath;
        }

        public List<int> GetAllDataRevisions() =>
            GetAllDBFileNames()
                .Select(ExtractVersionFromFileName)
                    .ToList();

        public IEnumerable<FinVizDataItem> GetRevision(int version = 0)
        {
            if(version == 0)
                version = GetLastDBFileVersion();

            var dbFilePath = GetDBFilePathByVersion(version);
            if (string.IsNullOrEmpty(dbFilePath))
                return new List<FinVizDataItem>();

            using (var db = new LiteDatabase(dbFilePath))
            {
                var collection = db.GetCollection<FinVizDataItem>(_collectionName);
                return collection.FindAll().ToList();
            }
        }

        public int SaveData(IEnumerable<FinVizDataItem> data)
        {
            if (data?.Count() == null)
                return 0;
            var lastVersion = GetLastDBFileVersion();
            lastVersion++;
            var dbFileName = CreateNewDBFilePath(lastVersion);
            using (var db = new LiteDatabase(dbFileName))
            {
                var collection = db.GetCollection<FinVizDataItem>(_collectionName);
                collection.EnsureIndex(item => item.Id, unique: true);
                collection.EnsureIndex(item => item.Version);
                int newId = 1;
                foreach (var item in data)
                {
                    item.Id = newId;
                    item.Version = lastVersion;
                    newId++;
                }

                collection.InsertBulk(data);
            }
            return data.Count();
        }

        public Dictionary<string, List<FinVizDataItem>> GetTickerMappedData(int version = 0)
        {
            var allVersions = new List<int>();

            if (version != 0)
                allVersions.Add(version);
            else allVersions = GetAllDataRevisions();

            Dictionary<string, List<FinVizDataItem>> aggregatedMap = new();

            foreach (var versionNum in allVersions)
            {
                var map = GetVersionBasedTickerMappedData(versionNum);
                foreach (var item in map)
                {
                    if (!aggregatedMap.ContainsKey(item.Key))
                        aggregatedMap.Add(item.Key, 
                            new List<FinVizDataItem>() { item.Value });
                    else aggregatedMap[item.Key].Add(item.Value);
                }
            }

            return aggregatedMap;
        }

        private Dictionary<string, FinVizDataItem> GetVersionBasedTickerMappedData(int version)
        {
            var dbFileName = GetDBFilePathByVersion(version);
            Dictionary<string, FinVizDataItem> result = new();
            using (var db = new LiteDatabase(dbFileName))
            {
                var collection = db.GetCollection<FinVizDataItem>(_collectionName);
                result = collection.FindAll().ToDictionary(item => item.Ticker);
            }
            return result;
        }

        private string GetDBFilePathByVersion(int version)
        {
            if (!Directory.Exists(_dbPath))
            {
                throw new DirectoryNotFoundException($"The directory {_dbPath} does not exist.");
            }

            var files = Directory.GetFiles(_dbPath);

            if (files.Length == 0)
                return string.Empty;

            var versionedFiles = files
                .Select(filePath => new
                {
                    FilePath = filePath,
                    Version = ExtractVersionFromFileName(filePath)
                })
                .Where(f => f.Version == version)
                .FirstOrDefault();

            return versionedFiles?.FilePath ?? string.Empty;
        }

        private int ExtractVersionFromFileName(string? filePath)
        {
            if(filePath == null)
                return 0;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var versionPart = fileName.Split('_').FirstOrDefault(part => part.StartsWith("v", StringComparison.OrdinalIgnoreCase));
            return int.TryParse(versionPart?.TrimStart('v'), out int version) ? version : 0;
        }

        private string CreateNewDBFilePath(int versionNumber)
        {
            if (!Directory.Exists(_dbPath))
            {
                Directory.CreateDirectory(_dbPath);
            }
            var newFileName = $"v{versionNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.db";
            var newFilePath = Path.Combine(_dbPath, newFileName);

            return newFilePath;
        }

        private List<string> GetAllDBFileNames()
        {
            List<string> result = new();
            if (!Directory.Exists(_dbPath))
                return result;

            var files = Directory.GetFiles(_dbPath);
            if (files.Length == 0)
                return result;

            result = files
                .Where(f => !f.Contains("-log") &&
                    f.Contains(".db"))
                .OrderBy(filePath =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var match = Regex.Match(fileName, @"v(\d+)_");
                    return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
                })
                .ToList();

            return result;
        }

        private int GetLastDBFileVersion() =>
            ExtractVersionFromFileName(
                GetAllDBFileNames()
                    .LastOrDefault());

        Dictionary<string, FinVizDataItem> IFinvizDBAdapter.GetTickerMappedData(int version)
        {
            throw new NotImplementedException();
        }
    }
}
