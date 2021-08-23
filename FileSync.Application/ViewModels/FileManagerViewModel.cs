using FileSync.Application.Interfaces;
using FileSync.DomainModel.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace FileSync.Application.ViewModels
{
    public class FileManagerViewModel : IFileManagerViewModel
    {
        private readonly IIOService _iOService;
        public IList<SyncDirectory> SyncDirectories { get; set; }
        public SyncDirectory SelectedSyncDirectory { get; set; }

        public FileManagerViewModel(IIOService iOService)
        {
            _iOService = iOService;

            GetDirectorySettings();
        }

        public void SaveDirectorySettings()
        {
            string serializedSyncDirectories = JsonSerializer.Serialize(SyncDirectories);

            _iOService.SaveDirectorySettings(serializedSyncDirectories);
        }
        public void GetDirectorySettings()
        {
            SyncDirectories = JsonSerializer.Deserialize<List<SyncDirectory>>(_iOService.GetDirectorySettings());
            NotifyPropertyChanged(nameof(SyncDirectories));
        }

        public void AddDirectory(SyncDirectory syncDirectory)
        {
            syncDirectory.Directory = syncDirectory.Directory.Replace("\\", "/");
            SyncDirectories.Add(syncDirectory);
            SaveDirectorySettings();
            GetDirectorySettings();
        }
        public void DeleteDirectory(SyncDirectory syncDirectory)
        {
            SyncDirectories.Remove(syncDirectory);
            SaveDirectorySettings();
            GetDirectorySettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
