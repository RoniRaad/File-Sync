using System.ComponentModel;

namespace FileSync
{
    public interface ILoginViewModel
    {
        string DisplayText { get; set; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}