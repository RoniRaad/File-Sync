using FileSync.Application.Interfaces;
using FileSync.Application.ViewModels;
using FileSync.DomainModel.Models;
using FileSync.Infrastructure.Extensions;
using FileSync.Infrastructure.Models;
using FileSync.Infrastructure.Services;
using FileSync.WindowsService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;

namespace FileSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context, services);
                })
                .Build();
        }


        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<AzureAdConfig>(context.Configuration.GetSection("AzureAdConfig"));
            services.AddTransient<IIOService, IOService>();
            services.AddTransient<IFileManagerViewModel, FileManagerViewModel>();
            services.AddTransient<ITokenCacheService, TokenCacheService>();
            services.AddTransient<ILoginViewModel, LoginViewModel>();
            services.AddSingleton<IAuthorizationService, AzureADService>();
            services.AddSingleton<IAddDirectoryViewModel, AddDirectoryViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<FileManager>();
            services.AddSavePathConfig();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            Window startingWindow;
            var azureAdService = _host.Services.GetService<IAuthorizationService>();
            var successfulSilentSignIn = await azureAdService.TrySilentSignIn();

            if (!successfulSilentSignIn)
                startingWindow = _host.Services.GetRequiredService<MainWindow>();
            else
                startingWindow = _host.Services.GetRequiredService<FileManager>();

            startingWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }


    }
}
