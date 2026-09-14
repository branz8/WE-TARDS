using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class DownloadStaffDocument
    {
        private readonly ILogger<DownloadStaffDocument> _logger;
        private readonly ShareClient _shareClient;

        public DownloadStaffDocument(ILogger<DownloadStaffDocument> logger)
        {
            _logger = logger;

            // Use FileShareStorage connection string
            var connectionString = Environment.GetEnvironmentVariable("FileShareStorage");
            _shareClient = new ShareClient(connectionString, "staff-docs");
            _shareClient.CreateIfNotExists();
        }

        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
            string fileName)
        {
            _logger.LogInformation($"DownloadStaffDocument triggered for: {fileName}");

            try
            {
                var directory = _shareClient.GetRootDirectoryClient();
                var fileClient = directory.GetFileClient(fileName);

                // Check if file exists
                if (!await fileClient.ExistsAsync())
                {
                    return new NotFoundObjectResult($"File '{fileName}' not found in staff-docs.");
                }

                // Download the file
                var memoryStream = new MemoryStream();
                var download = await fileClient.DownloadAsync();
                await download.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Determine content type
                var contentType = "application/octet-stream";
                if (fileName.EndsWith(".pdf"))
                    contentType = "application/pdf";
                else if (fileName.EndsWith(".txt"))
                    contentType = "text/plain";
                else if (fileName.EndsWith(".docx"))
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                // Return the file
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