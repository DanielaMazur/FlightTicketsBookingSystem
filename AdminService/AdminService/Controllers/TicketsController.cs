using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AdminService.Interfaces;
using AdminService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AdminService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     [Authorize]
     public class TicketsController : ControllerBase
     {
          private readonly ITicketsService _ticketsService;
          private readonly TaskTimeoutsConfig _taskTimeoutsConfig;

          public TicketsController(ITicketsService ticketsService, IConfiguration configuration)
          {
               _ticketsService = ticketsService;
               _taskTimeoutsConfig = configuration.GetSection("TasksTimeouts").Get<TaskTimeoutsConfig>();
          }

          [HttpPost]
          public async Task<IActionResult> CreateTicket(Ticket ticket)
          {
               try
               {
                    int timeout = _taskTimeoutsConfig.CreateTicket;
                    var task = _ticketsService.CreateTicket(ticket);
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                         return Ok(task.Result);
                    }

                    return StatusCode(StatusCodes.Status408RequestTimeout);
               }
               catch (BadHttpRequestException exception)
               {
                    return BadRequest(exception.Message);
               }
          }

          [HttpPatch("{id}")]
          public async Task<IActionResult> UpdateTicket(string id, UpdateTicket updatedTicket)
          {
               try
               {
                    int timeout = _taskTimeoutsConfig.UpdateTicket;
                    var task = _ticketsService.UpdateTicket(id, updatedTicket);
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                         return Ok(task.Result);
                    }

                    return StatusCode(StatusCodes.Status408RequestTimeout);
               }
               catch (BadHttpRequestException exception)
               {
                    return BadRequest(exception.Message);
               }
          }

          [HttpDelete("{id}")]
          public async Task<IActionResult> DeleteTicket(string id)
          {
               int timeout = _taskTimeoutsConfig.DeleteTicket;
               var task = _ticketsService.DeleteTicket(id);
               if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
               {
                    return NoContent();
               }

               return StatusCode(StatusCodes.Status408RequestTimeout);
          }
     }
}
