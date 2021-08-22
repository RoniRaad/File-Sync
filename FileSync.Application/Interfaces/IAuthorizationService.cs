using System.Threading.Tasks;

namespace FileSync.Application.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsSignedIn { get; set; }

        Task<string> GetAccessToken();
        Task SignIn();
        Task<bool> TrySilentSignIn();
        public Task SignOut();
    }
}