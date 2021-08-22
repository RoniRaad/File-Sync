using FileSync.DomainModel.Enums;
using System.ComponentModel;

namespace FileSync.DomainMode.Models
{
    public class SyncDirectory
    {
        public string SyncId { get; set; }
        public string Directory { get; set; }
        public RecursionType RecursionType { get; set; }
    }

}
