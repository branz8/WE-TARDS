using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Menu
{
    public class UpdateMenuItem
    {
        private readonly ILogger _logger;
        private readonly TableStorageService _tableService;

        public UpdateMenuItem(ILoggerFactory loggerFactory, TableStorageService tableService)
        {
            _logger = loggerFactory.CreateLogger<UpdateMenuItem>();
            _tableService = tableService;
        }

        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category,
            string id)
        {
            _logger.LogInformation($"Processing UpdateMenuItem request for {category}/{id}");

            try
            {
                var existingItem = await _tableService.GetMenuItemAsync(category, id);
                if (existingItem == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Menu item with Category '{category}' and Id '{id}' not found.");
                    return notFoundResponse;
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateData = JsonSerializer.Deserialize<MenuItemDto>(requestBody);

                if (updateData == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid request data.");
                    return badResponse;
                }

                existingItem.Price = updateData.Price > 0 ? updateData.Price : existingItem.Price;
                existingItem.IsAvailable = updateData.IsAvailable;

                if (!string.IsNullOrEmpty(updateData.Name))
                    existingItem.Name = updateData.Name;
                if (!string.IsNullOrEmpty(updateData.Description))
                    existingItem.Description = updateData.Description;

                await _tableService.UpdateMenuItemAsync(existingItem);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { message = "Menu item updated successfully", data = existingItem });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating menu item: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}