using FileSync.Application.Interfaces;
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
        private static readonly string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync");
        private static readonly string _savePath = Path.Combine(_saveDirectory, "syncDirs.json");

        public IOService()
        {
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
            if (!File.Exists(_savePath))
                File.WriteAllText(_savePath, "[]");
        }
        public void SaveDirectorySettings(string syncDirectories)
        {
            File.WriteAllText(_savePath, syncDirectories);
        }

        public string GetDirectorySettings()
        {
            return File.ReadAllText(_savePath);
        }
    }
}
