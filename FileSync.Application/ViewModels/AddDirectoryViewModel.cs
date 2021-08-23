using FileSync.Application.Interfaces;
using FileSync.DomainModel.Models;
using System.ComponentModel;

namespace FileSync.Application.ViewModels
{
    public class AddDirectoryViewModel : INotifyPropertyChanged, IAddDirectoryViewModel
    {
        public SyncDirectory SyncDirectory { get; set; } = new SyncDirectory();

        public void SyncDirectoryUpdated()
        {
            NotifyPropertyChanged(nameof(SyncDirectory));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
