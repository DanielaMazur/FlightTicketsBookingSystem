using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using CacheService.ExtensionMethods;
using CacheService.Middlewares;
using CacheService.Services;
using StackExchange.Redis;

namespace CacheService
{
     public class Startup
     {
          public Startup(IConfiguration configuration)
          {
               Configuration = configuration;
          }

          public IConfiguration Configuration { get; }

          // This method gets called by the runtime. Use this method to add services to the container.
          public void ConfigureServices(IServiceCollection services)
          {

               services.AddControllers();
               services.AddSwaggerGen(c =>
               {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CacheService", Version = "v1" });
               });
               services.AddScoped<CurrentServiceRequest>();
               services.AddSingleton<Services.CacheService>();

               //var santinel = ConnectionMultiplexer.SentinelConnect("redis-sentinel:26379");
               //ConnectionMultiplexer.Connect("redis-master:6379,redis-replica-1:6380,redis-replica-2:6381")
               //ConfigurationOptions sentinelConfig = new ConfigurationOptions
               //{
               //     ServiceName = "mymaster",
               //     CommandMap = CommandMap.Sentinel,
               //};
               //sentinelConfig.EndPoints.Add("redis-sentinel", 26379);
               //sentinelConfig.EndPoints.Add("redis-sentinel-1", 26379);
               //sentinelConfig.EndPoints.Add("redis-sentinel-2", 26379);

               //var masterConfig = new ConfigurationOptions()
               //{
               //     ServiceName = "mymaster",
               //     Password = "my_master_password"
               //};
               //masterConfig.EndPoints.Add("redis-master", 6379);
               ////masterConfig.EndPoints.Add("redis-slave", 6379);
               //masterConfig.EndPoints.Add("redis-replica-1", 6379);
               //masterConfig.EndPoints.Add("redis-replica-2", 6379);

               //var multiplexer = ConnectionMultiplexer.SentinelConnect(sentinelConfig);
               //services.AddScoped<ConnectionMultiplexer>(_ => multiplexer);
               services.AddDistributedMemoryCache();
          }

          // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CacheService v1"));
               }

               app.UseHttpsRedirection();

               app.UseRouting();

               //app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

               app.UseAuthorization();

               app.RegisterServiceToDiscovery(Configuration);

               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapControllers();
               });
          }
     }
}
