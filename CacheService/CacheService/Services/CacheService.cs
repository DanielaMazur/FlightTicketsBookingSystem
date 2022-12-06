using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheService.Services
{
     public class CacheService
     {
          private readonly ConnectionMultiplexer _connectionMultiplexer;

          public CacheService()
          {
               ConfigurationOptions sentinelConfig = new ConfigurationOptions
               {
                    ServiceName = "mymaster",
                    CommandMap = CommandMap.Sentinel,
               };
               sentinelConfig.EndPoints.Add("redis-sentinel", 26379);
               sentinelConfig.EndPoints.Add("redis-sentinel-1", 26379);
               sentinelConfig.EndPoints.Add("redis-sentinel-2", 26379);
               sentinelConfig.EndPoints.Add("redis-sentinel-3", 26379);

               _connectionMultiplexer = ConnectionMultiplexer.SentinelConnect(sentinelConfig);
               _connectionMultiplexer.ConfigurationChanged +=
                    ((sender, args) => Console.WriteLine("master changed!!!!!!!!!"));
          }

          public async Task<T> GetAsync<T>(string key)
               where T : class
          {
               var masterConfig = new ConfigurationOptions()
               {
                    ServiceName = "mymaster",
                    Password = "my_master_password"
               };
               var db = _connectionMultiplexer.GetSentinelMasterConnection(masterConfig).GetDatabase();

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
               var masterConfig = new ConfigurationOptions()
               {
                    ServiceName = "mymaster",
                    Password = "my_master_password"
               };
               var db = _connectionMultiplexer.GetSentinelMasterConnection(masterConfig).GetDatabase();
               var objString = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
               await db.StringSetAsync(key, objString, TimeSpan.FromMinutes(3));
          }
     }
}
