using System.Collections.Generic;
using System.Threading.Tasks;
using AdminService.Models;

namespace AdminService.Interfaces
{
     public interface ICosmosDbService<T> where T: Entity
     {
          Task<IEnumerable<T>> GetMultipleAsync(string query);

          Task<T> GetAsync(string id);

          Task<T> AddAsync(T item);

          Task UpdateAsync(string id, T item);

          Task DeleteAsync(string id);
     }
}
