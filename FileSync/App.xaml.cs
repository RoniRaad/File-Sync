using FileSync.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
                    ConfigureServices(services);
                })
                .Build();
        }


        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IIOService, IOService>();
            services.AddTransient<IFileManagerViewModel, FileManagerViewModel>();
            services.AddTransient<ILoginViewModel, LoginViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<AzureADService>();
            services.AddSingleton<FileManager>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var azureAdService = _host.Services.GetService<AzureADService>();
            var successfulSilentSignIn = await azureAdService.TrySilentSignIn();
            Window startingWindow;

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
