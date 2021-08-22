using System.ComponentModel;

namespace FileSync.Application.Interfaces
{
    public interface ILoginViewModel
    {
        string DisplayText { get; set; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}