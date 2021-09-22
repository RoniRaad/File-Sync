using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;
using System.Text.Json;
using Azure.Storage.Blobs.Models;
using FileSync.DomainModel.Models;
using Microsoft.AspNetCore.StaticFiles;
using FileSync.API.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using FileSync.API.Extensions;

namespace FileSync.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope("access_as_user")]
    public class FileSyncStorageController : ControllerBase
    {
        private readonly ILogger<FileSyncStorageController> _logger;
        private readonly StorageAccountConfig _storageAccountConfig;
        private readonly BlobServiceClient _blobServiceClient;

        public FileSyncStorageController(ILogger<FileSyncStorageController> logger, 
            IOptions<StorageAccountConfig> storageAccountConfig)
        {
            _logger = logger;
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
            try
            {
                _logger.LogInformation("Receiving incoming file from client...");
                BlobContainerClient blobContainer;
                FSFileInfo fileInfo = JsonSerializer.Deserialize<FSFileInfo>(parameters["FSFileInfo"]);
                _logger.LogInformation("Deserialized file info...");

                var currentUser = User;
                var tags = new Dictionary<string, string>();

                tags.Add("LastModifiedTime", fileInfo.ModifiedTime.Ticks.ToString());
                string name = currentUser.Identity.Name.GetBlobContainerUID();

                blobContainer = _blobServiceClient.GetBlobContainerClient(name);

                if (!await blobContainer.ExistsAsync())
                    blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

                _logger.LogInformation("Created BlobContainer...");

                var blobClient = blobContainer.GetBlobClient(Path.Combine(fileInfo.Path, Path.GetFileName(file.FileName)));

                await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);
                _logger.LogInformation("File uploaded successfully to storage account...");

                blobClient.SetTags(tags);
                _logger.LogInformation("Successfully applied new tags to file!");

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.InnerException?.Message);
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        [HttpGet("getfile", Name = "getfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFile(string filePath)
        {
            try
            {
                _logger.LogInformation("Receiving a file request from client...");

                BlobContainerClient blobContainer;
                ClaimsPrincipal currentUser = User;
                string sanitizedFilePath = filePath.SanitizeDirectory();
                string name = currentUser.Identity.Name.GetBlobContainerUID();

                blobContainer = _blobServiceClient.GetBlobContainerClient(name);

                if (!await blobContainer.ExistsAsync())
                    blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);

                _logger.LogInformation("Blob container created successfully...");

                var blobClient = blobContainer.GetBlobClient(filePath);

                if (!await blobClient.ExistsAsync())
                    return BadRequest();

                _logger.LogInformation("Blob client created successfully and file is found in storage account...");

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(sanitizedFilePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                _logger.LogInformation("File headers set successfully...");

                var bytes = await blobClient.OpenReadAsync();
                _logger.LogInformation("File read from storage account successfully!");

                return File(bytes, contentType);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.InnerException?.Message);
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        [HttpGet("getmodifiedtimes", Name = "getmodifiedtimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModifiedTimes()
        {
            try
            {
                _logger.LogInformation("Receiving a file last modified attribute request...");

                BlobContainerClient blobContainer;

                ClaimsPrincipal currentUser = User;
                IDictionary<string, FSFileInfo> fileInfo = new Dictionary<string, FSFileInfo>();
                string name = currentUser.Identity.Name.GetBlobContainerUID();

                blobContainer = _blobServiceClient.GetBlobContainerClient(name);

                if (!await blobContainer.ExistsAsync())
                    blobContainer = await _blobServiceClient.CreateBlobContainerAsync(name);
                _logger.LogInformation("Blob container created successfully...");

                await foreach (var blobItem in blobContainer.GetBlobsAsync(traits: BlobTraits.Tags))
                {
                    fileInfo.Add(blobItem.Name, new FSFileInfo()
                    {
                        FileName = Path.GetFileName(blobItem.Name),
                        Path = Path.GetDirectoryName(blobItem.Name),
                        ModifiedTime = new DateTime(long.Parse(blobItem.Tags["LastModifiedTime"]))
                    });
                }
                _logger.LogInformation("Successfully read attribute from all files in clients container!");

                return Ok(JsonSerializer.Serialize(fileInfo));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.InnerException?.Message);
                _logger.LogError(e.StackTrace);
                throw;
            }
        }
    }
}
