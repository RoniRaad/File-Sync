using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSync.WindowsService.Models
{
    class AzureAdConfig
    {
        public string AADInstance { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string FileSyncScope { get; set; }
        public string FileSyncBaseAddress { get; set; }
    }
}
