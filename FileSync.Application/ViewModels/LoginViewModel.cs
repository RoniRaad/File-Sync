using System.ComponentModel;


namespace FileSync
{
    public class LoginViewModel : INotifyPropertyChanged, ILoginViewModel
    {
        private string _displayText;
        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                if (_displayText == value)
                    return;
                _displayText = value;
                NotifyPropertyChanged(nameof(DisplayText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
