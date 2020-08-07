using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Middleware.ProviderFactory;

namespace Wiz.Template.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build()/*.SeedData()*/.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var settings = config.Build();
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(settings["ConnectionStrings:AppConfig"])
                           .Select(KeyFilter.Any, context.HostingEnvironment.EnvironmentName.ToLower())
                           .ConfigureRefresh(refresh =>
                           {
                               refresh.Register($"{Startup.SQUAD}:Settings:Sentinel", label: context.HostingEnvironment.EnvironmentName.ToLower(), refreshAll: true)
                                      .SetCacheExpiration(new TimeSpan(0, 30, 0));
                           });
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .UseServiceProviderFactory(
                new TenantServiceProviderFactory<Tenant>(Startup.ConfigureTenantServices)
            );
    }
}
