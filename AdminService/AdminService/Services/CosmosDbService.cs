using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminService.Interfaces;
using AdminService.Models;
using Microsoft.Azure.Cosmos;

namespace AdminService.Services
{
     public class CosmosDbService<T>: ICosmosDbService<T> where T : Entity
     {
               private readonly Container _container;

               public CosmosDbService(
                    CosmosClient cosmosDbClient,
                    string databaseName,
                    string containerName)
               {
                    _container = cosmosDbClient.GetContainer(databaseName, containerName);
               }

               public async Task<T> AddAsync(T item)
               {
                    item.Id = Guid.NewGuid().ToString();
                    return await _container.CreateItemAsync(item, new PartitionKey(item.Id));
               }

               public async Task DeleteAsync(string id)
               {
                    await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
               }

               public async Task<T> GetAsync(string id)
               {
                    try
                    {
                         var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
                         return response.Resource;
                    }
                    catch (CosmosException) //For handling item not found and other exceptions
                    {
                         return null;
                    }
               }

               public async Task<IEnumerable<T>> GetMultipleAsync(string queryString)
               {
                    var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));

                    var results = new List<T>();
                    while (query.HasMoreResults)
                    {
                         var response = await query.ReadNextAsync();
                         results.AddRange(response.ToList());
                    }

                    return results;
               }

               public async Task UpdateAsync(string id, T item)
               {
                    await _container.UpsertItemAsync(item, new PartitionKey(id));
               }
          }
}
