using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace CacheService
{
     public class Program
     {
          public static void Main(string[] args)
          {
               //configure logging first
               ConfigureLogging();

               //then create the host, so that if the host fails we can log errors
               CreateHost(args);
          }

          private static void CreateHost(string[] args)
          {
               try
               {
                    Console.WriteLine("GetEnvironmentVariable {0}", Environment.GetEnvironmentVariable("app_port"));
                    CreateHostBuilder(args).Build().Run();
               }
               catch (Exception ex)
               {
                    Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
                    throw;
               }
          }

          public static IHostBuilder CreateHostBuilder(string[] args) =>
               Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                         webBuilder.UseStartup<Startup>();
                    })
                    .ConfigureAppConfiguration(configuration =>
                    {
                         configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                         configuration.AddJsonFile(
                              $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                              optional: true);
                    })
                    .UseSerilog();

          private static void ConfigureLogging()
          {
               var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
               var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(
                         $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                         optional: true)
                    .Build();

               Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                    .Enrich.WithProperty("Environment", environment)
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
          }

          private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
          {
               Console.WriteLine("Elastic Search URL {0}", configuration["ElasticConfiguration:Uri"]);
               return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
               {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
               };
          }
     }
}
