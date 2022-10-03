using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
          public TicketsController(
               ILogger<TicketsController> logger,
               ICosmosDbService<Ticket> ticketCosmosDbService)
          {
               _logger = logger;
               _ticketCosmosDbService = ticketCosmosDbService;
          }

          [HttpGet]
          public async Task<IEnumerable<Ticket>> GetTickets()
          {
               return await _ticketCosmosDbService.GetAllAsync();
          }
     }
}
