using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSync.WindowsService
{
    public class FileSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public FileSyncService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<string> SyncFiles()
        {
            return "test";
        }
    }
    
}
