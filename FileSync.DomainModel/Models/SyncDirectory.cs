using FileSync.DomainModel.Enums;
using System.ComponentModel;

namespace FileSync
{
    public class SyncDirectory
    {
        public string SyncId { get; set; }
        public string Directory { get; set; }
        public RecursionType RecursionType { get; set; }
    }

}
