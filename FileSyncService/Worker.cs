using FileSync.WindowsService;
using FileSync.WindowsService.Models;
using FileSync.WindowsService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileSync.WindowsService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly FileSyncService _fileSyncService;
        private readonly ILogger<WindowsBackgroundService> _logger;
        private readonly AzureAdConfig _azureAdConfig;
        public WindowsBackgroundService(
            FileSyncService fileSyncService,
            ILogger<WindowsBackgroundService> logger,
            IOptions<AzureAdConfig> options) =>
            (_fileSyncService, _logger, _azureAdConfig) = (fileSyncService, logger, options.Value);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string status = await _fileSyncService.SyncFiles();
                    _logger.LogWarning(status);

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}