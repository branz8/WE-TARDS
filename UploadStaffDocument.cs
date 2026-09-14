using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions
{
    public class UploadStaffDocument
    {
        private readonly ILogger<UploadStaffDocument> _logger;
        private readonly ShareClient _shareClient;

        public UploadStaffDocument(ILogger<UploadStaffDocument> logger)
        {
            _logger = logger;

            // Use FileShareStorage connection string (full Azurite connection with FileEndpoint)
            var connectionString = Environment.GetEnvironmentVariable("FileShareStorage");
            _shareClient = new ShareClient(connectionString, "staff-docs");

            // Create share if it doesn't exist
            _shareClient.CreateIfNotExists();
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req)
        {
            _logger.LogInformation("UploadStaffDocument function triggered");

            try
            {
                // Check if file is present in the request
                var file = req.Form.Files["file"];
                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("No file uploaded. Please include a 'file' in multipart/form-data.");
                }

                var fileName = file.FileName;
                var directory = _shareClient.GetRootDirectoryClient();
                var fileClient = directory.GetFileClient(fileName);

                // Upload the file
                using (var stream = file.OpenReadStream())
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
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