using System.Collections.Generic;
using AuthService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class ServiceInfoController : ControllerBase
     {
          private readonly IEventsService _eventService;

          public ServiceInfoController(
               IEventsService eventService)
          {
               _eventService = eventService;
          }

          [HttpGet]
          public Dictionary<string, int> GetServiceInfo()
          {
               return _eventService.Events;
          }
     }
}
