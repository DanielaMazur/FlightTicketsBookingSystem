using System.Threading.Tasks;
using CustomerService.Models;

namespace CustomerService.Interfaces
{
     public interface ICacheService
     {
          Task<T> GetCacheData<T>(string key);
          Task PostCacheData<T>(CacheItem<T> cacheItem);
     }
}
