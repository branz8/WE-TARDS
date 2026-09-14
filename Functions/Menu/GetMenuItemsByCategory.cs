using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Menu
{
    public class GetMenuItemsByCategory
    {
        private readonly ILogger _logger;
        private readonly TableStorageService _tableService;

        public GetMenuItemsByCategory(ILoggerFactory loggerFactory, TableStorageService tableService)
        {
            _logger = loggerFactory.CreateLogger<GetMenuItemsByCategory>();
            _tableService = tableService;
        }

        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            _logger.LogInformation($"Processing GetMenuItemsByCategory request for category: {category}");

            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Category is required.");
                    return badResponse;
                }

                var menuItems = await _tableService.GetMenuItemsByCategoryAsync(category);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(menuItems);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting menu items by category: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}