using FinVizDataService.Models;
using LiteDB;

namespace FinVizScreener.DB
{
    public class LocalLiteDBSeparateFilesAdapter : IFinvizDBAdapter
    {
        private readonly string _dbPath;
        private const string CollectionName = "FinVizData";

        public LocalLiteDBSeparateFilesAdapter(string dbPath)
        {
            _dbPath = dbPath;
        }

        public IEnumerable<FinVizDataItem> GetLatestData()
        {
            var version = GetLastDBFileVersion();
            var dbFilePath = GetDBFilePathByVersion(version);
            if (string.IsNullOrEmpty(dbFilePath))
                return new List<FinVizDataItem>();

            using (var db = new LiteDatabase(dbFilePath))
            {
                var collection = db.GetCollection<FinVizDataItem>(CollectionName);
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
                var collection = db.GetCollection<FinVizDataItem>(CollectionName);
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

        private int ExtractVersionFromFileName(string filePath)
        {
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

        private int GetLastDBFileVersion()
        {
            if (!Directory.Exists(_dbPath))
                return 0;

            var files = Directory.GetFiles(_dbPath);
            if (files.Length == 0)
                return 0;

            var versions = files
                .Select(filePath =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (fileName.Contains("-log"))
                        return 0;
                    return ExtractVersionFromFileName(fileName);
                })
                .Where(version => version > 0)
                .ToList();

            return versions.Any() ? versions.Max() : 0;
        }
    }
}
