using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class ListStaffDocuments
    {
        private readonly ILogger<ListStaffDocuments> _logger;
        private readonly BlobContainerClient _containerClient;

        public ListStaffDocuments(ILogger<ListStaffDocuments> logger)
        {
            _logger = logger;

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient("staff-docs");
            _containerClient.CreateIfNotExists();
        }

        [Function("ListStaffDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
        {
            _logger.LogInformation("ListStaffDocuments function triggered");

            try
            {
                var documents = new List<object>();

                await foreach (var blobItem in _containerClient.GetBlobsAsync())
                {
                    var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                    var properties = await blobClient.GetPropertiesAsync();

                    documents.Add(new
                    {
                        fileName = blobItem.Name,
                        fileSize = properties.Value.ContentLength,
                        lastModified = properties.Value.LastModified,
                        fileType = properties.Value.ContentType ?? "application/octet-stream"
                    });
                }

                return new OkObjectResult(new
                {
                    count = documents.Count,
                    documents = documents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing documents: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}