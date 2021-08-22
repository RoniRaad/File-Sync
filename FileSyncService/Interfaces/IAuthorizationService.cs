using System.Threading.Tasks;

namespace FileSync.WindowsService.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsSignedIn { get; set; }

        Task<string> GetAccessToken();
        Task<bool> TrySilentSignIn();
    }
}