using System.Collections.Generic;
using CacheService.Models;

namespace CacheService.Services
{
     public class CacheService
     {
          public IDictionary<string, IList<CacheItem>> Cache = new Dictionary<string, IList<CacheItem>>();
     }
}
