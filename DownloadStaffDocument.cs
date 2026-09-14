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
    public class DownloadStaffDocument
    {
        private readonly ILogger<DownloadStaffDocument> _logger;
        private readonly BlobContainerClient _containerClient;

        public DownloadStaffDocument(ILogger<DownloadStaffDocument> logger)
        {
            _logger = logger;

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient("staff-docs");
            _containerClient.CreateIfNotExists();
        }

        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
            string fileName)
        {
            _logger.LogInformation($"DownloadStaffDocument triggered for: {fileName}");

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    return new NotFoundObjectResult($"File '{fileName}' not found in staff-docs.");
                }

                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                var contentType = "application/octet-stream";
                if (fileName.EndsWith(".pdf"))
                    contentType = "application/pdf";
                else if (fileName.EndsWith(".txt"))
                    contentType = "text/plain";
                else if (fileName.EndsWith(".docx"))
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                return new FileStreamResult(memoryStream, contentType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}