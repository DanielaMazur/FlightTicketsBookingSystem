using System.Threading.Tasks;
using AdminService.Models;

namespace AdminService.Interfaces
{
     public interface ITicketsService
     {
          Task<Ticket> CreateTicket(Ticket ticket);

          Task<Ticket> UpdateTicket(string id, UpdateTicket updatedTicket);

          Task DeleteTicket(string id);
     }
}
