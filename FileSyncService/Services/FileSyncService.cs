﻿using FileSync.Infrastructure.Services;
using FileSync.WindowsService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace FileSync.WindowsService
{
    public class FileSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly AzureAdConfig _azureAdConfig;
        private readonly IAuthorizationService _authorizationService;

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public FileSyncService(HttpClient httpClient, IOptions<AzureAdConfig> azureAdConfig, IAuthorizationService authorizationService)
        {
            _httpClient = httpClient;
            _azureAdConfig = azureAdConfig.Value;
            _authorizationService = authorizationService;
        }
        public async Task<string> SyncFiles()
        {
            return await UploadFile("C:\\Users\\rraad\\Desktop\\New folder\\ljM610_611_612_fs5.2_fw_2502087_007683_MD5_Checksum.txt");
        }
        public async Task<string> UploadFile(string filePath)
        {
            // _logger.LogInformation($"Uploading a text file [{filePath}].");
            var _url = "https://localhost:44336/FileSyncStorage/upload";
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File [{filePath}] not found.");
            }
            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filePath));
//          form.Add(new StringContent("789"), "userId");
//          form.Add(new StringContent("some comments"), "comment");
//          form.Add(new StringContent("true"), "isPrimary");

            var response = await _httpClient.PostAsync($"{_url}", form);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            //_logger.LogInformation("Uploading is complete.");
            return responseContent;
        }

    }
    
}
