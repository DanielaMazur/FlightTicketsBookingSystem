using System.Linq;
using System.Reflection;
using AuthService.ExtensionMethods;
using AuthService.Interface;
using AuthService.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace AuthService
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
               var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

               var builder = services.AddIdentityServer(options =>
                    {
                         options.Events.RaiseSuccessEvents = true;
                         options.Events.RaiseFailureEvents = true;
                         options.Events.RaiseErrorEvents = true;
                         options.Events.RaiseInformationEvents = true;
                    })
                    .AddConfigurationStore(options =>
                    {
                         options.ConfigureDbContext = b => b.UseSqlServer(Configuration.GetConnectionString("AuthDB"),
                              sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                    .AddOperationalStore(options =>
                    {
                         options.ConfigureDbContext = b => b.UseSqlServer(Configuration.GetConnectionString("AuthDB"),
                              sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                    .AddTestUsers(Config.GetUsers());

               builder.AddDeveloperSigningCredential();

               services.AddControllers();
               services.AddSingleton<IEventSink, EventsService>();
               services.AddSingleton<IEventsService>((x) => (IEventsService)x.GetService<IEventSink>());
          }

          // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {

               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
               }

               app.UseIdentityServer();

               app.UseRouting();

               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapControllers();
               });

               app.RegisterServiceToDiscovery(Configuration);

               InitializeDatabase(app);
          }

          private void InitializeDatabase(IApplicationBuilder app)
          {
               using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
               serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

               var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
               context.Database.Migrate();
               if (!context.Clients.Any())
               {
                    foreach (var client in Config.Clients)
                    {
                         context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
               }

               if (!context.IdentityResources.Any())
               {
                    foreach (var resource in Config.IdentityResources)
                    {
                         context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
               }

               if (!context.ApiScopes.Any())
               {
                    foreach (var resource in Config.ApiScopes)
                    {
                         context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
               }
          }
     }
}
