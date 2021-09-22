using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSync.Infrastructure.Models
{
    public class SavePathConfig
    {
        public string Path { get; set; }
        public string SyncDirectoriesFileName { get; set; }
        public string TokenCacheFileName { get; set; }

    }
}
