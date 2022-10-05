using System.Threading.Tasks;
using CacheService.Models;
using CacheService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CacheService.Middlewares
{
     public class ApiKeyAuthenticationMiddleware
     {
          
          private readonly RequestDelegate _next;
          private const string ApiKey = "X-API-Key";
          private const string ApiKeysSection = "ApiKeys";
          
          public ApiKeyAuthenticationMiddleware(RequestDelegate next)
          {
               _next = next;
          }

          public async Task InvokeAsync(HttpContext context)
          {
               var currentServiceRequest = context.RequestServices.GetRequiredService<CurrentServiceRequest>();

               if (!context.Request.Headers.TryGetValue(ApiKey, out var extractedApiKey))
               {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Api Key was not provided ");
                    return;
               }

               var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
               var apiKeyConf = appSettings.GetSection(ApiKeysSection).Get<ApiKeyConf>();

               var currentServiceName = GetCurrentServiceName(extractedApiKey, apiKeyConf);

               if (currentServiceName == null) 
               {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized client");
                    return;
               }

               currentServiceRequest.ServiceName = currentServiceName;

               await _next(context);
          }

          private string GetCurrentServiceName(string apiKey, ApiKeyConf apiKeyConf)
          {
               if (apiKey == apiKeyConf.CustomerService)
               {
                    return Const.CustomerService;
               }

               return null;
          }
     }
}
