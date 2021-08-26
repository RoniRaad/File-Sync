using FileSync.DomainModel.Models;
using FileSync.WindowsService.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSync.WindowsService.Services
{
    public class FileSyncService
    {
        private readonly string _savePath = Path.Combine(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync"),
        "syncDirs.json");
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly AzureAdConfig _azureAdConfig;
        private readonly IAuthorizationService _authorizationService;

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FileSyncService(ILogger<FileSyncService> logger, HttpClient httpClient, IOptions<AzureAdConfig> azureAdConfig, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _azureAdConfig = azureAdConfig.Value;
            _authorizationService = authorizationService;
        }

        public async Task SyncFiles()
        {
            try
            {
                var syncDirectories = await GetSyncDirectories();
                _logger.LogInformation("Remote Sync directories retrieved successfully...");
                var localSyncFiles = GetDictionaryOfFileInfos(syncDirectories);
                _logger.LogInformation("Local Sync directories retrieved successfully...");
                var remoteSyncFiles = await GetDictionaryOfRemoteFileInfo();
                _logger.LogInformation("Dictionary of remote file info retrieved successfully...");


                foreach (var localSyncDirectory in localSyncFiles)
                    try
                    {
                        if (remoteSyncFiles.ContainsKey(localSyncDirectory.Key))
                        {
                            if (localSyncDirectory.Value.ModifiedTime.CompareTo(remoteSyncFiles[localSyncDirectory.Key].ModifiedTime) > 0)
                                await UploadFile(localSyncDirectory.Value);
                            else if (localSyncDirectory.Value.ModifiedTime.CompareTo(remoteSyncFiles[localSyncDirectory.Key].ModifiedTime) < 0)
                                await RequestFile(localSyncDirectory.Value);
                            remoteSyncFiles.Remove(localSyncDirectory.Key);
                        }
                        else
                            await UploadFile(localSyncDirectory.Value);
                        _logger.LogInformation($"Successfully compaired file {localSyncDirectory.Key} to remote file...");

                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error when compairing file {localSyncDirectory.Key} to remote file...");
                        _logger.LogError(e.Message);
                        _logger.LogError(e.InnerException?.Message);
                        _logger.LogError(e.StackTrace);
                    }

                if (remoteSyncFiles.Count > 0)
                    foreach (var remoteSyncFile in remoteSyncFiles)
                        if (syncDirectories.Count((syncDir) => syncDir.SyncId == GetRootFolder(remoteSyncFile.Value.Path)) > 0)
                            await RequestFile(remoteSyncFile.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.InnerException?.Message);
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        private async Task RequestFile(FSFileInfo fileInfo)
        {
            var url = $"{_azureAdConfig.FileSyncBaseAddress}FileSyncStorage/getfile?filePath={Path.Combine(fileInfo.Path, fileInfo.FileName)}";

            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


            var response = await _httpClient.GetAsync($"{url}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsByteArrayAsync();

            var localPath = await GetLocalPath(fileInfo);

            File.WriteAllBytes(localPath, responseContent);
            File.SetLastWriteTime(localPath, fileInfo.ModifiedTime);
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

        private IDictionary<string, FSFileInfo> GetDictionaryOfFileInfos(IList<SyncDirectory> syncDirectories)
        {
            var fileInfo = new Dictionary<string, FSFileInfo>();

            foreach (var syncDirectory in syncDirectories)
                foreach (var file in Directory.GetFiles(syncDirectory.Directory, "*.*", SearchOption.AllDirectories))
                    fileInfo.Add( Path.Combine(syncDirectory.SyncId, Path.GetRelativePath(syncDirectory.Directory, file)).Replace("\\", "/"), new FSFileInfo
                    {
                        FileName = Path.GetFileName(file),
                        ModifiedTime = File.GetLastWriteTime(file),
                        Path = Path.Combine(syncDirectory.SyncId ,Path.GetDirectoryName(Path.GetRelativePath(syncDirectory.Directory, file))).Replace("\\", "/")
                    });

            return fileInfo;
        }

        public async Task<string> UploadFile(FSFileInfo filePath)
        {
            var url = $"{_azureAdConfig.FileSyncBaseAddress}FileSyncStorage/upload";

            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(await GetLocalPath(filePath)));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filePath.FileName));
            form.Add(new StringContent(JsonSerializer.Serialize(filePath)), "FSFileInfo");

            var response = await _httpClient.PostAsync($"{url}", form);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        public async Task<string> GetLocalPath(FSFileInfo fileInfo)
        {
            var syncDirs = await GetSyncDirectories();
            var matchingDirectory = syncDirs.Where((dir) => dir.SyncId == GetRootFolder(fileInfo.Path)).FirstOrDefault();
            return Path.Combine(fileInfo.Path.Replace(matchingDirectory.SyncId, matchingDirectory.Directory), fileInfo.FileName);
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
            var url = $"{_azureAdConfig.FileSyncBaseAddress}FileSyncStorage/getmodifiedtimes";

            string accessToken = await _authorizationService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{url}");
            response.EnsureSuccessStatusCode();
            var responseContent = (await response.Content.ReadAsStringAsync()).ToString();

            return JsonSerializer.Deserialize<Dictionary<string ,FSFileInfo>>(responseContent);
        }

    }
    
}
