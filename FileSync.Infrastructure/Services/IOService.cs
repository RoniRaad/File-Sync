using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileSync
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    /// 
    public class IOService : IIOService
    {
        private static readonly string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FileSync");
        private static readonly string _savePath = Path.Combine(_saveDirectory, "syncDirs.json");

        public IOService()
        {
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
            if (!File.Exists(_savePath))
                File.WriteAllText(_savePath, "[]");
        }
        public void SaveDirectorySettings(IList<SyncDirectory> syncDirectories)
        {
            string serializedSyncDirectories = JsonSerializer.Serialize(syncDirectories);
            File.WriteAllText(_savePath, serializedSyncDirectories);
        }

        public IList<SyncDirectory> GetDirectorySettings()
        {
            string serializedSyncDirectories = File.ReadAllText(_savePath);
            return JsonSerializer.Deserialize<List<SyncDirectory>>(serializedSyncDirectories);
        }
    }
}
