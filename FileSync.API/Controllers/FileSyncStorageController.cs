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
            tags.Add("LastModified", DateTime.Now.ToString());
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

        [HttpGet("checkmtime", Name = "checkmtime")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(
         IFormFile file,
         IFormCollection keyValuePairs,
         CancellationToken cancellationToken)
        {
            BlobContainerClient blobContainer;
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string sanitizedFilePath = SanitizeFolderName(keyValuePairs["FilePath"]);


            return Ok();
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
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }
    }
}
