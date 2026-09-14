using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Menu
{
    public class CreateMenuItem
    {
        private readonly ILogger _logger;
        private readonly TableStorageService _tableService;

        public CreateMenuItem(ILoggerFactory loggerFactory, TableStorageService tableService)
        {
            _logger = loggerFactory.CreateLogger<CreateMenuItem>();
            _tableService = tableService;
        }

        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Processing CreateMenuItem request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var menuItemDto = JsonSerializer.Deserialize<MenuItemDto>(requestBody);

                if (menuItemDto == null || string.IsNullOrEmpty(menuItemDto.Category) ||
                    string.IsNullOrEmpty(menuItemDto.Id))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid request. Category and Id are required.");
                    return badResponse;
                }

                var exists = await _tableService.MenuItemExistsAsync(menuItemDto.Category, menuItemDto.Id);
                if (exists)
                {
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await conflictResponse.WriteStringAsync($"Item with Category '{menuItemDto.Category}' and Id '{menuItemDto.Id}' already exists.");
                    return conflictResponse;
                }

                var menuItem = new MenuItem
                {
                    PartitionKey = menuItemDto.Category,
                    RowKey = menuItemDto.Id,
                    Name = menuItemDto.Name,
                    Description = menuItemDto.Description,
                    Price = menuItemDto.Price,
                    IsAvailable = menuItemDto.IsAvailable
                };

                await _tableService.AddMenuItemAsync(menuItem);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(new { message = "Menu item created successfully", data = menuItem });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating menu item: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}