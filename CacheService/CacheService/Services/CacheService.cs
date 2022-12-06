using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheService.Services
{
     public class CacheService
     {
          private readonly IConnectionMultiplexer _connectionMultiplexer;

          public CacheService(IConnectionMultiplexer connectionMultiplexer)
          {
               _connectionMultiplexer = connectionMultiplexer;
          }

          public async Task<T> GetAsync<T>(string key)
               where T : class
          {
               var db = _connectionMultiplexer.GetDatabase();

               var data = await db.StringGetAsync(key);

               if (!data.HasValue)
               {
                    return null;
               }

               var obj = JsonSerializer.Deserialize<T>(data);
               return obj;
          }

          public async Task SetAsync<T>(string key, T data)
               where T : class
          {
               var db = _connectionMultiplexer.GetDatabase();
               var objString = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
               await db.StringSetAsync(key, objString, TimeSpan.FromMinutes(3));
          }
     }
}
