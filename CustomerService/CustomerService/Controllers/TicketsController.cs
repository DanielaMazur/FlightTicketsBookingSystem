using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.Interfaces;
using CustomerService.Models;

namespace CustomerService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     public class TicketsController : ControllerBase
     {
          private readonly ILogger<TicketsController> _logger;
          private readonly ICosmosDbService<Ticket> _ticketCosmosDbService;
          private readonly ICacheService _cacheService;

          public TicketsController(
               ILogger<TicketsController> logger,
               ICosmosDbService<Ticket> ticketCosmosDbService,
               ICacheService cacheService)
          {
               _logger = logger;
               _ticketCosmosDbService = ticketCosmosDbService;
               _cacheService = cacheService;
          }

          [HttpGet]
          public async Task<IEnumerable<Ticket>> GetTickets()
          {
               var cacheData = await _cacheService.GetCacheData<IEnumerable<Ticket>>("tickets");
               if (cacheData == null)
               {
                    _logger.LogInformation("Get tickets from the DB");
                    var tickets = (await _ticketCosmosDbService.GetAllAsync()).ToList();
                    await _cacheService.PostCacheData(new CacheItem<IEnumerable<Ticket>>()
                    {
                         Key = "tickets",
                         Cache = tickets
                    });

                    return tickets;
               }

               _logger.LogInformation("Get tickets from the Cache");
               return cacheData;
          }
     }
}
