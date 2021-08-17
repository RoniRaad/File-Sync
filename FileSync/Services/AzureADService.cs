using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;

namespace FileSync.Infrastructure.Services
{
    public class AzureADService : IAuthorizationService
    {
        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);
        private static readonly string TodoListScope = ConfigurationManager.AppSettings["FileSync:FileSyncScope"];
        private static readonly string TodoListBaseAddress = ConfigurationManager.AppSettings["FileSync:FileSyncBaseAddress"];
        private static readonly string[] Scopes = { TodoListScope };
        private readonly IPublicClientApplication _app;
        private string AccessToken;
        public bool IsSignedIn { get; set; }

        public AzureADService()
        {
            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost")
                .Build();

            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
        }

        public async Task<string> GetAccessToken()
        {
            if (AccessToken is null)
                await SignIn();

            return AccessToken;
        }

        public async Task<bool> TrySilentSignIn()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            accounts = (await _app.GetAccountsAsync()).ToList();
            try
            {
                var result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                AccessToken = result.AccessToken;

                IsSignedIn = true;

                return IsSignedIn;
            }
            catch (MsalUiRequiredException ex)
            {
                Console.WriteLine(ex.Message);
                return IsSignedIn;
            }
        }
        public async Task SignIn()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            var silentSignInSuccessful = await TrySilentSignIn();

            if (!silentSignInSuccessful)
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

                    AccessToken = result.AccessToken;

                    IsSignedIn = true;
                    // Logged in

                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode != "access_denied")
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                    }

                    IsSignedIn = false;
                }
            }
        }
    }
}
