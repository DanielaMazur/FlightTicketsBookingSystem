using System.Threading.Tasks;
using AdminService.Interfaces;
using AdminService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AdminService.Services
{
     public class TicketsService: ITicketsService
     {
          private readonly ILogger<ITicketsService> _logger;
          private readonly ICosmosDbService<Ticket> _ticketsCosmosDbService;

          public TicketsService(
               ILogger<ITicketsService> logger,
               ICosmosDbService<Ticket> ticketsCosmosDbService)
          {
               _logger = logger;
               _ticketsCosmosDbService = ticketsCosmosDbService;
          }

          public async Task<Ticket> CreateTicket(Ticket ticket)
          {
               if (ticket.FromAirportId == null || ticket.ToAirportId == null || ticket.FromAirportId == ticket.ToAirportId)
               {
                    _logger.LogError("Some required ticket info is missing");
                    throw new BadHttpRequestException("Some required ticket info is missing");
               }

               var createdTicket = await _ticketsCosmosDbService.AddAsync(ticket);
               _logger.LogInformation($"Ticket with id {createdTicket.Id} created successfully");
               return createdTicket;
          }

          public async Task<Ticket> UpdateTicket(string id, UpdateTicket updatedTicket)
          {
               if (id == null)
               {
                    _logger.LogError("Ticket id is null");
                    throw new BadHttpRequestException("Ticket id is null");
               }

               var oldTicket = await _ticketsCosmosDbService.GetAsync(id);
               if (oldTicket == null)
               {
                    _logger.LogError("The ticket with id {0} that was required to be updated, doesn't exist", id);
                    throw new BadHttpRequestException($"The ticket with id {id} that was required to be updated, doesn't exist");
               }

               if (updatedTicket.DepartureTime.HasValue)
               {
                    oldTicket.DepartureTime = updatedTicket.DepartureTime.Value;
               }

               if (updatedTicket.Price.HasValue)
               {
                    oldTicket.Price = updatedTicket.Price.Value;
               }
               await _ticketsCosmosDbService.UpdateAsync(id, oldTicket);
               return oldTicket;
          }

          public async Task DeleteTicket(string id)
          {
               if (id == null)
               {
                    _logger.LogInformation("The ticket with id {0} that was requested to be deleted doesn't exist", id);
                    return;
               }
               await _ticketsCosmosDbService.DeleteAsync(id);
          }
     }
}
