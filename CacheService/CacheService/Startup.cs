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

               var redisOptions = ConfigurationOptions.Parse("cache:6379");
               redisOptions.Password = "RRnFPZ93tjBHB9W62p";
               redisOptions.AbortOnConnectFail = false;

               var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
               services.AddSingleton<IConnectionMultiplexer>(multiplexer);
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
