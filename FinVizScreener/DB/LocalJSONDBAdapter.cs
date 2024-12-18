﻿using FinVizDataService.Models;
using Newtonsoft.Json;

namespace FinVizScreener.DB
{
    public class LocalJSONDBAdapter : IFinvizDBAdapter
    {
        private readonly string _dbFolderPath;
        public LocalJSONDBAdapter(string dbFolderPath)
        {
            _dbFolderPath = dbFolderPath;
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }
        }

        public IEnumerable<FinVizDataItem> GetLatestData()
        {
            var data = LoadDataFromDB(
                GetLastModifiedFileName(_dbFolderPath));
            return data ?? new List<FinVizDataItem>();
        }

        public int SaveData(IEnumerable<FinVizDataItem> data)
        {
            if(data == null || data.Count() == 0) 
                return 0;
            var fPath = Path.Combine(_dbFolderPath,
                $"{data.First().Date:dd-MM-yyyy-hh-mm-ss}.json");
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fPath, jsonData);
            return data.Count();
        }

        private IEnumerable<FinVizDataItem>? LoadDataFromDB(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var jsonData = File.ReadAllText(fileName);
                var objData = JsonConvert.DeserializeObject<List<FinVizDataItem>>(jsonData);
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
