using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Menu
{
    public class DeleteMenuItem
    {
        private readonly ILogger _logger;
        private readonly TableStorageService _tableService;

        public DeleteMenuItem(ILoggerFactory loggerFactory, TableStorageService tableService)
        {
            _logger = loggerFactory.CreateLogger<DeleteMenuItem>();
            _tableService = tableService;
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category,
            string id)
        {
            _logger.LogInformation($"Processing DeleteMenuItem request for {category}/{id}");

            try
            {
                var exists = await _tableService.MenuItemExistsAsync(category, id);
                if (!exists)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Menu item with Category '{category}' and Id '{id}' not found.");
                    return notFoundResponse;
                }

                await _tableService.DeleteMenuItemAsync(category, id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { message = $"Menu item with Category '{category}' and Id '{id}' deleted successfully" });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting menu item: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}