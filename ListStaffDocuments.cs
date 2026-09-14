using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class ListStaffDocuments
    {
        private readonly ILogger<ListStaffDocuments> _logger;
        private readonly ShareClient _shareClient;

        public ListStaffDocuments(ILogger<ListStaffDocuments> logger)
        {
            _logger = logger;

            // Use FileShareStorage connection string
            var connectionString = Environment.GetEnvironmentVariable("FileShareStorage");
            _shareClient = new ShareClient(connectionString, "staff-docs");
            _shareClient.CreateIfNotExists();
        }

        [Function("ListStaffDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
        {
            _logger.LogInformation("ListStaffDocuments function triggered");

            try
            {
                var documents = new List<object>();
                var directory = _shareClient.GetRootDirectoryClient();

                // List all files in the root
                await foreach (var fileItem in directory.GetFilesAndDirectoriesAsync())
                {
                    if (!fileItem.IsDirectory)
                    {
                        var fileClient = directory.GetFileClient(fileItem.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        documents.Add(new
                        {
                            fileName = fileItem.Name,
                            fileSize = properties.Value.ContentLength,
                            lastModified = properties.Value.LastModified,
                            fileType = properties.Value.ContentType ?? "application/octet-stream"
                        });
                    }
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