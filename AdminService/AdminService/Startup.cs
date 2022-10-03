using AdminService.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using AdminService.Interfaces;
using AdminService.Models;
using AdminService.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;

namespace AdminService
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

               services.AddControllers().AddNewtonsoftJson();
               services.AddSwaggerGen(c =>
               {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AdminService", Version = "v1" });
               });
               services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                         var identityConfig = Configuration.GetSection("Identity");
                         var identityAuthority = identityConfig["Authority"];

                         options.Authority = identityAuthority;

                         options.TokenValidationParameters = new TokenValidationParameters
                         {
                              ValidateAudience = false
                         };
                    });

               var cosmosConfig = Configuration.GetSection("CosmosDb");
               var cosmosClient = GetCosmosClientInstance(cosmosConfig);
               var cosmosDbName = cosmosConfig["DatabaseName"];

               services.AddSingleton<ICosmosDbService<Ticket>>(
                    new CosmosDbService<Ticket>(cosmosClient, cosmosDbName, "tickets"));
               services.AddSingleton<ICosmosDbService<Airport>>(
                    new CosmosDbService<Airport>(cosmosClient, cosmosDbName, "airports"));
          }

          // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdminService v1"));
               }

               app.UseHttpsRedirection();

               app.UseRouting();

               app.UseAuthentication();
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
