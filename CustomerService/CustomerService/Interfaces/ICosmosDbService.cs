using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerService.Models;

namespace CustomerService.Interfaces
{
     public interface ICosmosDbService<T> where T: Entity
     {
          Task<IEnumerable<T>> GetMultipleAsync(string query);

          Task<T> GetAsync(string id);

          Task<IEnumerable<T>> GetAllAsync();

          Task<T> AddAsync(T item);

          Task UpdateAsync(string id, T item);

          Task DeleteAsync(string id);
     }
}
