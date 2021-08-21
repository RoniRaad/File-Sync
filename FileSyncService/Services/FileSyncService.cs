using FileSync.DomainMode.Models;
using FileSync.Infrastructure.Services;
using FileSync.WindowsService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync");
        private static readonly string _savePath = Path.Combine(_saveDirectory, "syncDirs.json");

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
            var syncDirectories = await GetSyncDirectories();
            var localSyncFiles = await GetDictionaryOfFileInfos(syncDirectories);
            var remoteSyncFiles = await GetDictionaryOfRemoteFileInfo();

            foreach (var localSyncDirectory in localSyncFiles)
            {
                if (remoteSyncFiles.ContainsKey(localSyncDirectory.Key))
                {
                    if (localSyncDirectory.Value.ModifiedTime > remoteSyncFiles[localSyncDirectory.Key].ModifiedTime)
                        await RequestFile(localSyncDirectory.Value);
                    else
                        await UploadFile(localSyncDirectory.Value);

                    remoteSyncFiles.Remove(localSyncDirectory.Key);
                }
                else
                {
                    await UploadFile(localSyncDirectory.Value);
                }
            }

            if (remoteSyncFiles.Count > 0)
                foreach (var remoteSyncFile in remoteSyncFiles)
                {
                    await RequestFile(remoteSyncFile.Value);
                }

            return "";
        }

        private Task RequestFile(FSFileInfo value)
        {
            throw new NotImplementedException();
        }

        public List<FSFileInfo> GetFileInfoFromSyncDirectory(SyncDirectory directory)
        {
            var fileInfo = new List<FSFileInfo>();

            foreach (var file in Directory.GetFiles(directory.Directory, "*.*", SearchOption.AllDirectories))
            {
                var fsFileInfo = new FSFileInfo
                {
                    FileName = Path.GetFileName(file),
                    ModifiedTime = File.GetLastWriteTime(file),
                    Path = Path.GetDirectoryName(file),
                };

                fileInfo.Add(fsFileInfo);
            }

            return fileInfo;
        }

        private async Task<IList<SyncDirectory>> GetSyncDirectories()
        {
            var fileInfoJson = await File.ReadAllTextAsync(_savePath);
            return JsonSerializer.Deserialize<List<SyncDirectory>>(fileInfoJson);
        }

        private async Task<IDictionary<string, FSFileInfo>> GetDictionaryOfFileInfos(IList<SyncDirectory> syncDirectories)
        {
            
            var fileInfo = new Dictionary<string, FSFileInfo>();
            foreach (var syncDirectory in syncDirectories)
                foreach (var file in Directory.GetFiles(syncDirectory.Directory, "*.*", SearchOption.AllDirectories))
                    fileInfo.Add( Path.Combine(syncDirectory.SyncId, Path.GetRelativePath(syncDirectory.Directory, file)), new FSFileInfo
                    {
                        FileName = Path.GetFileName(file),
                        ModifiedTime = File.GetLastWriteTime(file),
                        Path = Path.Combine(syncDirectory.SyncId ,Path.GetRelativePath(syncDirectory.Directory, file))
                    });

            return fileInfo;
        }

        public async Task<string> UploadFile(FSFileInfo filePath)
        {
            // _logger.LogInformation($"Uploading a text file [{filePath}].");
            var _url = "https://localhost:44336/FileSyncStorage/upload";

            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(Path.Combine(filePath.Path, filePath.FileName)));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filePath.FileName));
            form.Add(new StringContent(JsonSerializer.Serialize(filePath)), "FSFileInfo");


            var response = await _httpClient.PostAsync($"{_url}", form);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            //_logger.LogInformation("Uploading is complete.");
            return responseContent;
        }

        public async Task<string> GetLocalPath(FSFileInfo fileInfo)
        {
            var syncDirs = await GetSyncDirectories();
            var matchingDirectory = syncDirs.Where((dir) => dir.SyncId == GetRootFolder(fileInfo.Path)).FirstOrDefault();
            return fileInfo.Path.Replace(matchingDirectory.SyncId, );
        }
        private static string GetRootFolder(string path)
        {
            var root = Path.GetPathRoot(path);
            while (true)
            {
                var temp = Path.GetDirectoryName(path);
                if (temp != null && temp.Equals(root))
                    break;
                path = temp;
            }
            return path;
        }
        public async Task<IDictionary<string, FSFileInfo>> GetDictionaryOfRemoteFileInfo()
        {
            // _logger.LogInformation($"Uploading a text file [{filePath}].");
            var _url = $"https://localhost:44336/FileSyncStorage/getmodifiedtimes";

            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_url}");
            response.EnsureSuccessStatusCode();
            var responseContent = (await response.Content.ReadAsStringAsync()).ToString();

            return JsonSerializer.Deserialize<Dictionary<string ,FSFileInfo>>(responseContent);
        }

    }
    
}
