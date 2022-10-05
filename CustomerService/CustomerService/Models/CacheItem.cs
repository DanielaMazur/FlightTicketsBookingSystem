namespace CustomerService.Models
{
     public class CacheItem<T>
     {
          public string Key { get; set; }

          public T Cache { get; set; }
     }
}
