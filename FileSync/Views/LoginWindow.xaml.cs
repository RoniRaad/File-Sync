using FileSync.Application.Interfaces;
using System.Windows;


namespace FileSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly FileManager _fileManagerWindow;
        private readonly ILoginViewModel _loginViewModel;
        private readonly IAuthorizationService _authorizationService;
        public MainWindow(FileManager fileManagerWindow, ILoginViewModel loginViewModel, IAuthorizationService authorizationService)
        {
            _fileManagerWindow = fileManagerWindow;
            _loginViewModel = loginViewModel;
            _authorizationService = authorizationService;

            InitializeComponent();

            DataContext = _loginViewModel;
            _loginViewModel.DisplayText = "You're not signed in!";
        }

        private async void SignIn(object sender, RoutedEventArgs e)
        {
            if (_loginViewModel.DisplayText == "You're not signed in!")
                await _authorizationService.SignIn();
            else
                await _authorizationService.SignOut();

            if (_authorizationService.IsSignedIn) 
            { 
                _fileManagerWindow.Show();
                Close();
            }

        }
    }
}
