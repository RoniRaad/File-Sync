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
        private readonly IPublicClientApplication _app;
        private readonly FileManager _fileManagerWindow;
        private readonly ILoginViewModel _loginViewModel;

        private bool IsLoggedIn = false;

        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);
        private static readonly string TodoListScope = ConfigurationManager.AppSettings["todo:FileSyncScope"];
        private static readonly string TodoListBaseAddress = ConfigurationManager.AppSettings["todo:FileSyncBaseAddress"];
        private static readonly string[] Scopes = { TodoListScope };

        public MainWindow(FileManager fileManagerWindow, ILoginViewModel loginViewModel)
        {
            _fileManagerWindow = fileManagerWindow;
            _loginViewModel = loginViewModel;

            InitializeComponent();
            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost")
                .Build();

            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);

            DataContext = _loginViewModel;
            _loginViewModel.DisplayText = "You're not signed in!";

            Show();

            SignIn();

        }

        public async void SignIn()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            if (!IsLoggedIn)
            {
                // Get an access token to call the To Do list service.
                try
                {
                    var result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    Dispatcher.Invoke(() =>
                    {
                        _loginViewModel.DisplayText = $"Hello {result.Account.Username}!";
                        _fileManagerWindow.Show();
                        Close();
                        //loginText.Text = result.Account.Username;
                        //SignInButton.Content = ClearCacheString;
                        //SetUserName(result.Account);
                        //GetTodoList();
                    }
                    );
                }
                catch (MsalUiRequiredException)
                {
                    try
                    {
                        // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                        // and we don't necessarily want to re-sign-in the same user
                        var builder = _app.AcquireTokenInteractive(Scopes)
                            .WithAccount(accounts.FirstOrDefault())
                            .WithPrompt(Prompt.SelectAccount);

                        if (!_app.IsEmbeddedWebViewAvailable())
                        {
                            // You app should install the embedded browser WebView2 https://aka.ms/msal-net-webview2
                            // but if for some reason this is not possible, you can fall back to the system browser 
                            // in this case, the redirect uri needs to be set to "http://localhost"
                            builder = builder.WithUseEmbeddedWebView(false);
                        }

                        var result = await builder.ExecuteAsync().ConfigureAwait(false);

                        Dispatcher.Invoke(() =>
                        {
                            _loginViewModel.DisplayText = $"Hello {result.Account.Username}!";

                            _fileManagerWindow.Show();
                            Close();
                            //SignInButton.Content = ClearCacheString;
                            //SetUserName(result.Account);
                            //GetTodoList();
                        }
                        );
                    }
                    catch (MsalException ex)
                    {
                        if (ex.ErrorCode == "access_denied")
                        {
                            // The user canceled sign in, take no action.
                        }
                        else
                        {
                            // An unexpected error occurred.
                            string message = ex.Message;
                            if (ex.InnerException != null)
                            {
                                message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                            }

                            MessageBox.Show(message);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            //UserName.Content = Properties.Resources.UserNotSignedIn;
                        });
                    }
                }
            }
        }
        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (_loginViewModel.DisplayText == "You're not signed in!")
                SignIn();
            else
                SignOut();
        }

        private async void SignOut()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            // Clears the library cache. Does not affect the browser cookies.
            while (accounts.Any())
            {
                await _app.RemoveAsync(accounts.First());
                accounts = (await _app.GetAccountsAsync()).ToList();
            }

            _loginViewModel.DisplayText = "You're not signed in!";
            return;
        }
    }
}
