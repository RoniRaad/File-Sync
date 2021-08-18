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
        private readonly BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

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
            System.Security.Claims.ClaimsPrincipal currentUser = User;

            string name = currentUser.Identity.Name.Split("#")[1].Replace("@", "at").Replace(".", "dot");

            BlobContainerClient blob = await blobServiceClient.CreateBlobContainerAsync(name);
            var bclient = blob.GetBlobClient("test");
            await bclient.UploadAsync("C:/Users/rraad/Desktop/New folder/test.txt", true);
           // await blobServiceClient.CreateBlobContainerAsync("tester");
           // Debug.WriteLine(currentUser.Identity.Name);
            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
            //    string value = reader.ReadToEnd();
            //    System.IO.File.WriteAllText("C:/Users/rraad/Desktop/New folder/test.txt", value);
                // Do something with the value
            }
            //return BadRequest(new { message = "Invalid file extension" });

            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
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
    }
}
