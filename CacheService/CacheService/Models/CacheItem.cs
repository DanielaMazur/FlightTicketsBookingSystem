using System;

namespace CacheService.Models
{
     public class CacheItem
     {
          public CacheItem()
          {
               ExpirationTime = DateTime.Now.AddMinutes(1);
          }

          public string Key { get; set; }

          public string Cache { get; set; }

          public DateTime ExpirationTime { get; }
     }
}
