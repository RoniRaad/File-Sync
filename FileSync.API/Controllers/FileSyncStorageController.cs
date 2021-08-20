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

namespace FileSync.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope("access_as_user")]
    public class FileSyncStorageController : ControllerBase
    {
        private static string connectionString = "";

        private readonly ILogger<FileSyncStorageController> _logger;
        private readonly BlobServiceClient _blobServiceClient = new BlobServiceClient(connectionString);

        public FileSyncStorageController(ILogger<FileSyncStorageController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
         IFormFile file,
         IFormCollection keyValuePairs,
         CancellationToken cancellationToken)
        {
            BlobContainerClient blobContainer;
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string sanitizedFilePath = SanitizeFolderName(keyValuePairs["FilePath"]);
            var tags = new Dictionary<string, string>();
            tags.Add("LastModifiedTime", keyValuePairs["LastModifiedTime"]);
            // Because a container can only have an lowercase alpha-numeric name with 63 characters maximum we truncate the hash and set all characters to their lowercase form.
            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();

            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            var blobClient = blobContainer.GetBlobClient(sanitizedFilePath[3..] + Path.GetFileName(file.FileName));

            await blobClient.UploadAsync(file.OpenReadStream());
            blobClient.SetTags(tags);


            return Ok();
        }

        [HttpGet("getmodifiedtime", Name = "getmodifiedtime")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetModifiedTime(string filePath, string fileName)
        {
            BlobContainerClient blobContainer;
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string sanitizedFilePath = SanitizeFolderName(filePath);
            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();

            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            var blobClient = blobContainer.GetBlobClient(sanitizedFilePath[3..] + Path.GetFileName(fileName));

            IDictionary<string,string> blobTags = (await blobClient.GetTagsAsync()).Value.Tags;

            return Ok(blobTags.ToList());
        }

        class FSFileInfo
        {
            public string Path { get; set; }
            public string FileName { get; set; }
            public string RelativePath { get; set; }
            public DateTime ModifiedTime { get; set; }
        }

        [HttpGet("getmodifiedtimes", Name = "getmodifiedtimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetModifiedTimes()
        {
            BlobContainerClient blobContainer;
            System.Security.Claims.ClaimsPrincipal currentUser = User;

            IList<FSFileInfo> fileInfo = new List<FSFileInfo>();

            string name = GetHashString(currentUser.Identity.Name).Substring(0, 63).ToLower();

            blobContainer = _blobServiceClient.GetBlobContainerClient(name);

            if (!await blobContainer.ExistsAsync())
                blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

            var blobs = blobContainer.GetBlobs();

            foreach (BlobItem blobItem in blobs)
            {
                fileInfo.Add(new FSFileInfo()
                {
                    FileName = Path.GetFileName(blobItem.Name),
                    RelativePath = Path.GetPathRoot(blobItem.Name),
                    Path = Path.GetDirectoryName(blobItem.Name),
                    ModifiedTime = DateTime.Parse(blobItem.Tags["LastModifiedTime"])
                });
            }
            return Ok(fileInfo);
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
