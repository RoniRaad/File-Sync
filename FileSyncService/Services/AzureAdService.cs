using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Options;
using FileSync.WindowsService.Interfaces;
using FileSync.DomainModel.Models;

namespace FileSync.WindowsService.Services
{
    public class AzureAdService : IAuthorizationService
    {
        private readonly AzureAdConfig _iDAConfig;

        private readonly string[] _scopes;
        private readonly IPublicClientApplication _app;
        private string AccessToken;
        public bool IsSignedIn { get; set; }

        public AzureAdService(IOptions<AzureAdConfig> iDAConfig)
        {
            _iDAConfig = iDAConfig.Value;
            _scopes = new string[] { _iDAConfig.FileSyncScope };

            _app = PublicClientApplicationBuilder.Create(_iDAConfig.ClientId)
                .WithAuthority(string.Format(CultureInfo.InvariantCulture, _iDAConfig.AADInstance, _iDAConfig.Tenant))
                .WithRedirectUri("http://localhost")
                .Build();

            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
        }

        public async Task<string> GetAccessToken()
        {
            if (AccessToken is null)
                await TrySilentSignIn();

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
    }
}
