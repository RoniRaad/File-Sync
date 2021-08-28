// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Security.Cryptography;
using FileSync.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace FileSync
{
    public class TokenCacheService : ITokenCacheService
    {
        /// <summary>j
        /// Path to the token cache
        /// </summary>
        private readonly SavePathConfig _savePathConfig;
        private readonly string _cacheFilePath;


        private readonly object _fileLock;

        public TokenCacheService(IOptions<SavePathConfig> savePathConfig)
        {
            _savePathConfig = savePathConfig.Value;
            _cacheFilePath = Path.Combine(_savePathConfig.Path, _savePathConfig.TokenCacheFileName);
            _fileLock = new object();
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_fileLock)
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
                lock (_fileLock)
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
