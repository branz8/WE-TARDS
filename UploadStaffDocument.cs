using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class UploadStaffDocument
    {
        private readonly ILogger<UploadStaffDocument> _logger;
        private readonly BlobContainerClient _containerClient;

        public UploadStaffDocument(ILogger<UploadStaffDocument> logger)
        {
            _logger = logger;

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient("staff-docs");
            _containerClient.CreateIfNotExists();
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req)
        {
            _logger.LogInformation("UploadStaffDocument function triggered");

            try
            {
                var file = req.Form.Files["file"];
                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("No file uploaded. Please include a 'file' in multipart/form-data.");
                }

                var fileName = file.FileName;
                var blobClient = _containerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return new OkObjectResult(new
                {
                    message = $"File '{fileName}' uploaded successfully",
                    fileName = fileName,
                    fileSize = file.Length,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}