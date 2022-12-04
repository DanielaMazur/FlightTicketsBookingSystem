using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheService.Models;
using CacheService.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace CacheService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     public class CacheController : ControllerBase
     {
          private readonly ILogger<CacheController> _logger;
          private readonly CurrentServiceRequest _currentServiceRequest;
          private readonly IDistributedCache _distributedCache;
          private readonly Services.CacheService _cacheService;

          public CacheController(ILogger<CacheController> logger, 
               CurrentServiceRequest currentServiceRequest,
               IDistributedCache distributedCache,
               Services.CacheService cacheService)
          {
               _logger = logger;
               _currentServiceRequest = currentServiceRequest;
               _distributedCache = distributedCache;
               _cacheService = cacheService;
          }

          [HttpGet("{key}")]
          public async Task<object> Get(string key)
          {
               var result = await _cacheService.GetAsync<object>(key);

               //if (_cacheService.Cache.ContainsKey(_currentServiceRequest.ServiceName))
               //{
               //     var cachedItem = _cacheService.Cache[_currentServiceRequest.ServiceName].FirstOrDefault(cache => cache.Key == key);
               //     if (cachedItem?.ExpirationTime < DateTime.Now)
               //     {
               //          _logger.LogInformation($"{_currentServiceRequest.ServiceName} has saved cached data, but it expired.");
               //          return null;
               //     }
               //     _logger.LogInformation($"{_currentServiceRequest.ServiceName} has saved cached data");
               //     return cachedItem?.Cache;
               //}

               if(result == null)
               {
                    _logger.LogInformation($"{_currentServiceRequest.ServiceName} doesn't have saved cache data");

                    return null;
               }

               _logger.LogInformation($"{_currentServiceRequest.ServiceName} has saved cached data");
               return result;
          }

          [HttpPost]
          public async Task<IActionResult> Post(CacheItem cacheItem)
          {
               //if (_cacheService.Cache.ContainsKey(_currentServiceRequest.ServiceName))
               //{
               //     _logger.LogInformation($"Add Service {_currentServiceRequest.ServiceName} to cache data");
               //     _cacheService.Cache[_currentServiceRequest.ServiceName].Add(cacheItem);
               //     return Ok();
               //}

               _logger.LogInformation($"Add Cache Data to the existing {_currentServiceRequest.ServiceName}");
               //_cacheService.Cache.Add(new KeyValuePair<string, IList<CacheItem>>(
               //     _currentServiceRequest.ServiceName, 
               //     new List<CacheItem>()
               //     {
               //          cacheItem
               //     }));


               await _cacheService.SetAsync(cacheItem.Key, cacheItem.Cache);

               return Ok();
          }
     }
}
