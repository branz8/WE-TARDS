using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Menu
{
    public class GetAllMenuItems
    {
        private readonly ILogger _logger;
        private readonly TableStorageService _tableService;

        public GetAllMenuItems(ILoggerFactory loggerFactory, TableStorageService tableService)
        {
            _logger = loggerFactory.CreateLogger<GetAllMenuItems>();
            _tableService = tableService;
        }

        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Processing GetAllMenuItems request.");

            try
            {
                var menuItems = await _tableService.GetAllMenuItemsAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(menuItems);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting menu items: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}