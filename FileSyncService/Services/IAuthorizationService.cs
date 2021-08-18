using System.Threading.Tasks;

namespace FileSync.Infrastructure.Services
{
    public interface IAuthorizationService
    {
        bool IsSignedIn { get; set; }

        Task<string> GetAccessToken();
        Task<bool> TrySilentSignIn();
    }
}