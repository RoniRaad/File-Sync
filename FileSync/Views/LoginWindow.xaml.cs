using FileSync.Infrastructure.Services;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Formats.Asn1.AsnWriter;


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

        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);
        private static readonly string TodoListScope = ConfigurationManager.AppSettings["todo:FileSyncScope"];
        private static readonly string TodoListBaseAddress = ConfigurationManager.AppSettings["todo:FileSyncBaseAddress"];
        private static readonly string[] Scopes = { TodoListScope };

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
