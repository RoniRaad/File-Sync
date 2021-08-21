using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
