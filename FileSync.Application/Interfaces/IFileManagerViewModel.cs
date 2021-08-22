using FileSync.DomainMode.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace FileSync.Application.Interfaces
{
    public interface IFileManagerViewModel : INotifyPropertyChanged
    {
        IList<SyncDirectory> SyncDirectories { get; set; }
        SyncDirectory SelectedSyncDirectory { get; set; }

        void DeleteDirectory(SyncDirectory syncDirectory);
        void AddDirectory(SyncDirectory syncDirectory);
        void GetDirectorySettings();
        void SaveDirectorySettings();
    }
}