using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;
using CacheService.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheService.Services
{
     public class CacheService
     {
          private readonly IDistributedCache _distributedCache;
          private readonly DistributedCacheEntryOptions _options;

          public CacheService(IDistributedCache distributedCache)
          {
               _distributedCache = distributedCache;
               _options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
          }

          public IDictionary<string, IList<CacheItem>> Cache = new Dictionary<string, IList<CacheItem>>();

          public async Task<string> GetStringAsync(string key)
          {
               return await _distributedCache.GetStringAsync(key);
          }

          public async Task SetStringAsync(string key, string data)
          {
               await _distributedCache.SetStringAsync(key, data, _options);
          }

          public async Task<T> GetAsync<T>(string key)
               where T : class
          {
               var data = await _distributedCache.GetStringAsync(key);

               if(data == null)
               {
                    return null;
               }

               var obj = JsonSerializer.Deserialize<T>(data);

               //BinaryFormatter bf = new BinaryFormatter();
               //using MemoryStream ms = new MemoryStream(data);

               //var result = bf.Deserialize(ms);

               return obj;

               //ByteArrayInputStream inpStream = new ByteArrayInputStream(data);
               //ObjectInputStream objInpStream = new ObjectInputStream(inpStream);

               //return (T)objInpStream.readObject();
          }

          public async Task SetAsync<T>(string key, T data)
               where T : class
          {
               var objString = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });

               await _distributedCache.SetStringAsync(key, objString, _options);
          }
     }
}
