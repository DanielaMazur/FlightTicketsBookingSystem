using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DicoveryService.Models;
using Microsoft.Extensions.Configuration;

namespace DicoveryService.Controllers
{
     [ApiController]
     [Route("[controller]")]
     public class DiscoveryController : ControllerBase
     {
          private readonly ILogger<DiscoveryController> _logger;
          private static readonly HttpClient Client = new();

          private IList<ServiceInfo> _services = new List<ServiceInfo>();
          private readonly IConfiguration _config;

          public DiscoveryController(ILogger<DiscoveryController> logger, IConfiguration iConfig)
          {
               _config = iConfig;
               _logger = logger;
          }

          [HttpPost("/register")]
          public async Task<IActionResult> RegisterService(ServiceInfo serviceInfo)
          {
               _services.Add(serviceInfo);

               var json = JsonSerializer.Serialize(serviceInfo);
               var data = new StringContent(json, Encoding.UTF8, "application/json");

               var gatewayUrl = _config["GatewayUrl"];
               var gatewayRegisterServiceEndpoint = gatewayUrl + "registerService";
               await Client.PostAsync(gatewayRegisterServiceEndpoint, data);

               _logger.LogInformation("Service with id: {0} and name {1} was registered", serviceInfo.Id, serviceInfo.Name);
               return Ok();
          }
     }
}
