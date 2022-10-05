using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using CacheService.Models;
using CacheService.Services;

namespace CacheService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     public class CacheController : ControllerBase
     {
          private readonly ILogger<CacheController> _logger;
          private readonly CurrentServiceRequest _currentServiceRequest;
          private readonly Services.CacheService _cacheService;

          public CacheController(ILogger<CacheController> logger, 
               CurrentServiceRequest currentServiceRequest,
               Services.CacheService cacheService)
          {
               _logger = logger;
               _currentServiceRequest = currentServiceRequest;
               _cacheService = cacheService;
          }

          [HttpGet("{key}")]
          public string Get(string key)
          {
               if (_cacheService.Cache.ContainsKey(_currentServiceRequest.ServiceName))
               {
                    return _cacheService.Cache[_currentServiceRequest.ServiceName].FirstOrDefault(cache => cache.Key == key)?.Cache;
               }

               return null;
          }

          [HttpPost]
          public IActionResult Post(CacheItem cacheItem)
          {
               if(_cacheService.Cache.ContainsKey(_currentServiceRequest.ServiceName))
               {
                    _cacheService.Cache[_currentServiceRequest.ServiceName].Add(cacheItem);
                    return Ok();
               }

               _cacheService.Cache.Add(new KeyValuePair<string, IList<CacheItem>>(
                    _currentServiceRequest.ServiceName, 
                    new List<CacheItem>()
                    {
                         cacheItem
                    }));
               return Ok();
          }
     }
}
