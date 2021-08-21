using System.ComponentModel;

namespace FileSync.Application.ViewModels
{
    public interface IAddDirectoryViewModel
    {
        SyncDirectory SyncDirectory { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void SyncDirectoryUpdated();
    }
}