using FinVizDataService.Models;
using Newtonsoft.Json;

namespace FinVizScreener.DB
{
    public class LocalJSONFinvizDBAdapter : IFinvizDBAdapter
    {
        private readonly string _dbFolderPath;
        public LocalJSONFinvizDBAdapter(string dbFolderPath)
        {
            _dbFolderPath = dbFolderPath;
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }
        }

        public FinVizDataPack GetLatestData()
        {
            var data = LoadDataFromDB(
                GetLastModifiedFileName(_dbFolderPath));
            if (data != null)
                return data;
            
            data = new FinVizDataPack();
            data.FetchDate = DateTime.MinValue;
            data.Items = new List<FinVizDataItem>();
            return data;
        }

        public void SaveData(FinVizDataPack data)
        {
            var fPath = Path.Combine(_dbFolderPath, 
                $"{data.FetchDate:dd-MM-yyyy-hh-mm-ss}.json");
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fPath, jsonData);
        }

        private FinVizDataPack? LoadDataFromDB(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var jsonData = File.ReadAllText(fileName);
                var objData = JsonConvert.DeserializeObject<FinVizDataPack>(jsonData);
                if (objData != null)
                    return objData;
            }
            return null;
        }

        private string? GetLastModifiedFileName(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            if (!directoryInfo.Exists)
                return null;
            
            return directoryInfo.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault()?
                .Name;
        }
    }
}
