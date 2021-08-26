using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Identity.Client;
using System.Windows;
using FileSync.Application.Interfaces;
using Microsoft.Extensions.Options;
using FileSync.DomainModel.Models;

namespace FileSync.WindowsService.Services
{
    public class AzureADService : IAuthorizationService
    {
        private readonly AzureAdConfig _azureOptions;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly string _authority;
        private readonly string[] _scopes;
        private readonly IPublicClientApplication _app;
        private string AccessToken;
        public bool IsSignedIn { get; set; }

        public AzureADService(IOptions<AzureAdConfig> azureOptions, ITokenCacheService tokenCacheService)
        {
            _tokenCacheService = tokenCacheService;
            _azureOptions = azureOptions.Value;
            _authority = string.Format(CultureInfo.InvariantCulture, _azureOptions.AADInstance, _azureOptions.Tenant);
            _scopes = new string[] { _azureOptions.FileSyncScope };
            _app = PublicClientApplicationBuilder.Create(_azureOptions.ClientId)
                .WithAuthority(_authority)
                .WithRedirectUri("http://localhost")
                .Build();

            _tokenCacheService.EnableSerialization(_app.UserTokenCache);
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
                var result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
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
                    var builder = _app.AcquireTokenInteractive(_scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount);

                    if (!_app.IsEmbeddedWebViewAvailable())
                    {
                        builder = builder.WithUseEmbeddedWebView(false);
                    }

                    var result = await builder.ExecuteAsync().ConfigureAwait(false);

                    AccessToken = result.AccessToken;

                    IsSignedIn = true;

                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode != "access_denied")
                    {
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }
                        MessageBox.Show(message);
                    }

                    IsSignedIn = false;
                }
            }
        }

        public async Task SignOut()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();

            while (accounts.Any())
            {
                await _app.RemoveAsync(accounts.First());
                accounts = (await _app.GetAccountsAsync()).ToList();
            }

            IsSignedIn = false;
        }
    }
}
