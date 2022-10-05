﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AdminService.Interfaces;
using AdminService.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     [Authorize]
     public class TicketsController : ControllerBase
     {
          private readonly ILogger<TicketsController> _logger;
          private readonly ICosmosDbService<Ticket> _ticketsCosmosDbService;

          public TicketsController(
               ILogger<TicketsController> logger,
               ICosmosDbService<Ticket> ticketsCosmosDbService)
          {
               _logger = logger;
               _ticketsCosmosDbService = ticketsCosmosDbService;
          }

          [HttpPost]
          public async Task<IActionResult> CreateTicket(Ticket ticket)
          {
               if (ticket.FromAirportId == null || ticket.ToAirportId == null || ticket.FromAirportId == ticket.ToAirportId)
               {
                    return BadRequest();
               }
               return Ok(await _ticketsCosmosDbService.AddAsync(ticket));
          }
     }
}