using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using CustomerService.Interfaces;
using CustomerService.Models;
using CustomerService.Services;
using Microsoft.Azure.Cosmos;

namespace CustomerService
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
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerService", Version = "v1" });
               });

               var cosmosConfig = Configuration.GetSection("CosmosDb");
               var cosmosClient = GetCosmosClientInstance(cosmosConfig);
               var cosmosDbName = cosmosConfig["DatabaseName"];

               services.AddSingleton<ICosmosDbService<Ticket>>(
                    new CosmosDbService<Ticket>(cosmosClient, cosmosDbName, "tickets"));
               services.AddSingleton<ICosmosDbService<Airport>>(
                    new CosmosDbService<Airport>(cosmosClient, cosmosDbName, "airports"));
               services.AddScoped<ICacheService, CacheService>();
          }

          // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService v1"));
               }

               app.UseHttpsRedirection();

               app.UseRouting();

               app.UseAuthorization();

               app.RegisterServiceToDiscovery(Configuration);

               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapControllers();
               });
          }

          private static CosmosClient GetCosmosClientInstance(IConfigurationSection configurationSection)
          {
               var account = configurationSection["Account"];
               var key = configurationSection["Key"];

               return new CosmosClient(account, key);
          }
     }
}
