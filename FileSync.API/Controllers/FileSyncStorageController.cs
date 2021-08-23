using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;
using Azure.Storage.Blobs;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text.Json;
using Azure.Storage.Blobs.Models;
using FileSync.DomainModel.Models;
using Microsoft.AspNetCore.StaticFiles;
using FileSync.API.Models;
using Microsoft.Extensions.Options;

namespace FileSync.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope("access_as_user")]
    public class FileSyncStorageController : ControllerBase
    {
        private readonly StorageAccountConfig _storageAccountConfig;
        private readonly BlobServiceClient _blobServiceClient;

        public FileSyncStorageController(IOptions<StorageAccountConfig> storageAccountConfig)
        {
            _storageAccountConfig = storageAccountConfig.Value;
            _blobServiceClient = new BlobServiceClient(_storageAccountConfig.StorageAccountConnectionString);
        }

        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
         IFormFile file,
         IFormCollection parameters,
         CancellationToken cancellationToken)
        {
            BlobContainerClient blobContainer;
            FSFileInfo fileInfo = JsonSerializer.Deserialize<FSFileInfo>(parameters["FSFileInfo"]);
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var tags = new Dictionary<string, string>();

            tags.Add("LastModifiedTime", fileInfo.ModifiedTime.Ticks.ToString());
            // Because a container can only have an lowercase alpha-numeric name with 63 characters maximum we truncate the hash and set all characters to their lowercase form.
            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();

            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            var blobClient = blobContainer.GetBlobClient(Path.Combine(fileInfo.Path, Path.GetFileName(file.FileName)));

            await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);
            blobClient.SetTags(tags);

            return Ok();
        }

        [HttpGet("getfile", Name = "getfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFile(string filePath)
        {
            BlobContainerClient blobContainer;
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string sanitizedFilePath = SanitizeFolderName(filePath);
            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();
            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            var blobClient = blobContainer.GetBlobClient(filePath);

            if (!await blobClient.ExistsAsync())
                return BadRequest();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(sanitizedFilePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await blobClient.OpenReadAsync();
            return File(bytes, contentType);
        }

        [HttpGet("getmodifiedtimes", Name = "getmodifiedtimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModifiedTimes()
        {
            BlobContainerClient blobContainer;

            System.Security.Claims.ClaimsPrincipal currentUser = User;
            IDictionary<string ,FSFileInfo> fileInfo = new Dictionary<string, FSFileInfo>();
            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();
            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            await foreach (BlobItem blobItem in blobContainer.GetBlobsAsync(traits: BlobTraits.Tags))
            {
                fileInfo.Add(blobItem.Name ,new FSFileInfo()
                {
                    FileName = Path.GetFileName(blobItem.Name),
                    Path = Path.GetDirectoryName(blobItem.Name),
                    ModifiedTime = new DateTime(long.Parse(blobItem.Tags["LastModifiedTime"]))
                });
            }

            return Ok(JsonSerializer.Serialize(fileInfo));
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string SanitizeFolderName(string name)
        {
            string regexSearch = new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }

    }
}
