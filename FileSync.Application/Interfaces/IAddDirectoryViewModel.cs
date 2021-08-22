using FileSync.DomainMode.Models;
using System.ComponentModel;

namespace FileSync.Application.Interfaces
{
    public interface IAddDirectoryViewModel
    {
        SyncDirectory SyncDirectory { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void SyncDirectoryUpdated();
    }
}