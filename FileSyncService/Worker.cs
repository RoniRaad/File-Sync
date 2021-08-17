using FileSync.WindowsService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileSync.WindowsService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly FileSyncService _fileSyncService;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(
            FileSyncService jokeService,
            ILogger<WindowsBackgroundService> logger) =>
            (_fileSyncService, _logger) = (jokeService, logger);

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