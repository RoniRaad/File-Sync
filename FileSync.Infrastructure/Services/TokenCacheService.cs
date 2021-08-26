// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;

namespace FileSync
{
    public class TokenCacheService : ITokenCacheService
    {
        /// <summary>j
        /// Path to the token cache
        /// </summary>
        private readonly static string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync");
        private readonly static string _cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync.msalcache.bin");


        private static readonly object FileLock = new object();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists(_cacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath),
                                              null,
                                              DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(_cacheFilePath,
                                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                             null,
                                                             DataProtectionScope.CurrentUser)
                                      );
                }
            }
        }
        public void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
