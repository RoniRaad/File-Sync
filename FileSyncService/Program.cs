using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileSync.WindowsService;
using FileSync.WindowsService.Services;
using FileSync.WindowsService.Interfaces;
using FileSync.DomainModel.Models;
using FileSync;
using FileSync.Infrastructure.Models;
using System.IO;
using System;
using FileSync.Infrastructure.Extensions;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "File Sync Service";
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<WindowsBackgroundService>();
        services.AddHttpClient<FileSyncService>();
        services.Configure<AzureAdConfig>(context.Configuration.GetSection("AzureAdConfig"));
        services.AddSingleton<IAuthorizationService, AzureADService>();
        services.AddTransient<ITokenCacheService, TokenCacheService>();
        services.AddSavePathConfig();
    })
    .Build();

await host.RunAsync();