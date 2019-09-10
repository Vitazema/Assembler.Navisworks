using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Server.Assembler.Domain;

namespace Server.Assembler.Api
{
  public class Program
  {
    public static int Main(string[] args)
    {
      // Build configuration early for logging purposes
      var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
        .AddEnvironmentVariables()
        .Build();
      
      // Configure and push logger
      // When using ".UseSerilog()" it will use "Log.Logger"
      var logServer = configuration.GetSection("Logging")["LogstashServer"];
      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration.GetSection("Logging"))
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
          new Uri(logServer))
        {
          AutoRegisterTemplate = true,
          AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7
        })
        .CreateLogger();
      Log.Logger.Information("Logs are sending to {logServer}", logServer);

      // Start host with an application
      try
      {
        Log.Logger.Information("Startig host...");
        using (var webHost = BuildWebHost(args, configuration).Build())
        {
          webHost.Run();
        }
        return 0;
      }
      catch (Exception ex)
      {
        Log.Logger.Fatal(ex, "Application crashed unexpectedly");
        return 1;
      }
      finally
      {
        // Make sure all the log sinks have processes the last log before closing the application
        Log.CloseAndFlush();
      }
    }

    public static IWebHostBuilder BuildWebHost(string[] args, IConfiguration configuration)
    {
      var host = WebHost.CreateDefaultBuilder(args)
        .UseConfiguration(configuration)
        .ConfigureLogging((context, config) =>
        {
          config.ClearProviders();
          config.AddConfiguration(context.Configuration.GetSection("Logging"));
        })
        .UseKestrel()
        .UseStartup<Startup>()
        .UseSerilog();
      return host;
    }
  }
}

