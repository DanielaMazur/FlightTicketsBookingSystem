using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;

namespace CustomerService.ExtensionMethods
{
     public static class RegisterServiceExtension
     {
          public static async void RegisterServiceToDiscovery(this IApplicationBuilder app, IConfiguration config)
          {
               HttpClient client = new();
               var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
              //  var address = serverAddressesFeature.Addresses.First();

               var json = JsonSerializer.Serialize(new
               {
                    Id = Guid.NewGuid().ToString(),
                    Name = "CustomerService",
                    Url = "http://customerService:3000"
               });
               var data = new StringContent(json, Encoding.UTF8, "application/json");

               var gatewayUrl = config["DiscoveryUrl"];
               var registerEndpoint = gatewayUrl + "register";
               var response = await client.PostAsync(registerEndpoint, data);

               await response.Content.ReadAsStringAsync();
          }
     }
}
