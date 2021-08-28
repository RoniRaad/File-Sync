using FileSync.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSync.Infrastructure.Extensions
{
    public static class ServicesHostExtension
    {
        public static IServiceCollection AddSavePathConfig(this IServiceCollection services)
        {
            services.Configure<SavePathConfig>(opt =>
            {
                opt.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileSync");
                opt.TokenCacheFileName = "FileSync.msalcache.bin";
                opt.SyncDirectoriesFileName = "syncdirectories.json";
            });
            return services;
        }
    }
}
