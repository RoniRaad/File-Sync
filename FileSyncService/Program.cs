using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileSync.WindowsService;
using Microsoft.Extensions.Configuration;
using FileSync.WindowsService.Models;
using FileSync.Infrastructure.Services;

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
        services.AddSingleton<IAuthorizationService, HeadlessAzureADService>();
    })

    .Build();

await host.RunAsync();