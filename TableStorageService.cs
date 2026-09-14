using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;
        private const string TableName = "MenuItems";

        public TableStorageService(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient(TableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task AddMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.AddEntityAsync(menuItem);
        }

        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var results = new List<MenuItem>();
            await foreach (var entity in _tableClient.QueryAsync<MenuItem>())
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string category)
        {
            var results = new List<MenuItem>();
            var query = _tableClient.QueryAsync<MenuItem>(item => item.PartitionKey == category);

            await foreach (var entity in query)
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task<MenuItem> GetMenuItemAsync(string category, string id)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<MenuItem>(category, id);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return null;
            }
        }

        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.UpdateEntityAsync(menuItem, menuItem.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteMenuItemAsync(string category, string id)
        {
            await _tableClient.DeleteEntityAsync(category, id);
        }

        public async Task<bool> MenuItemExistsAsync(string category, string id)
        {
            var item = await GetMenuItemAsync(category, id);
            return item != null;
        }
    }
}