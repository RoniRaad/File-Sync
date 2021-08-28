using FileSync.Application.Interfaces;
using FileSync.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace FileSync.Infrastructure.Services
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    /// 
    public class IOService : IIOService
    {
        private readonly SavePathConfig _savePathConfig;
        private string syncDirectoryFilePath;
        public IOService(IOptions<SavePathConfig> savePathConfig)
        {
            _savePathConfig = savePathConfig.Value;
            syncDirectoryFilePath = Path.Combine(_savePathConfig.Path, _savePathConfig.SyncDirectoriesFileName);

            if (!Directory.Exists(_savePathConfig.Path))
                Directory.CreateDirectory(_savePathConfig.Path);
            if (!File.Exists(syncDirectoryFilePath))
                File.WriteAllText(syncDirectoryFilePath, "[]");
        }
        public void SaveDirectorySettings(string syncDirectories)
        {
            File.WriteAllText(syncDirectoryFilePath, syncDirectories);
        }

        public string GetDirectorySettings()
        {
            return File.ReadAllText(syncDirectoryFilePath);
        }
    }
}
