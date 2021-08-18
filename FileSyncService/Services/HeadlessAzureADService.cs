using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;
using System.Windows;
using FileSync.WindowsService.Models;
using Microsoft.Extensions.Options;

namespace FileSync.Infrastructure.Services
{
    public class HeadlessAzureADService : IAuthorizationService
    {
        private readonly AzureAdConfig _iDAConfig;

        private readonly string[] Scopes;
        private readonly IPublicClientApplication _app;
        private string AccessToken;
        public bool IsSignedIn { get; set; }

        public HeadlessAzureADService(IOptions<AzureAdConfig> iDAConfig)
        {
            _iDAConfig = iDAConfig.Value;
            Scopes = new string[] { _iDAConfig.FileSyncScope };

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
    }
}
