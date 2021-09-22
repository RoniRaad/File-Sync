using Microsoft.Identity.Client;

namespace FileSync
{
    public interface ITokenCacheService
    {
        void EnableSerialization(ITokenCache tokenCache);
    }
}