using System.Collections.Generic;
using System.ComponentModel;

namespace FileSync
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
            _iOService.SaveDirectorySettings(SyncDirectories);
        }
        public void GetDirectorySettings()
        {
            SyncDirectories = _iOService.GetDirectorySettings();
            NotifyPropertyChanged(nameof(SyncDirectories));
        }

        public void AddDirectory(SyncDirectory syncDirectory)
        {
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
