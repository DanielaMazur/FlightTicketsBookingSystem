using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CustomerService.Interfaces;
using CustomerService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CustomerService.Services
{
     public class CacheService: ICacheService
     {
          private readonly IConfiguration _config;
          private const string CacheServiceConfigKey = "CacheService";
          private const string ApiKeyHeader = "X-API-Key";

          private readonly HttpClient _httpClient = new();


          public CacheService(IConfiguration config)
          {
               _config = config;

               var cacheConfig = _config.GetSection(CacheServiceConfigKey);
               _httpClient.BaseAddress = new Uri(cacheConfig["Url"]);
               _httpClient.DefaultRequestHeaders.Add(ApiKeyHeader, cacheConfig["ApiKey"]);
          }

          public async Task<T> GetCacheData<T>(string key)
          {

               var response = await _httpClient.GetAsync($"cache/{key}");
               
               return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
          }

          public async Task PostCacheData<T>(CacheItem<T> cacheItem)
          {
               var cacheData = JsonSerializer.Serialize(cacheItem.Cache);
               var stringCacheItem = new CacheItem<string>()
               {
                    Key = cacheItem.Key,
                    Cache = cacheData
               };
               var json = JsonSerializer.Serialize(stringCacheItem);
               var data = new StringContent(json, Encoding.UTF8, "application/json");

               var response = await _httpClient.PostAsync("cache", data);

               await response.Content.ReadAsStringAsync();
          }
     }
}
