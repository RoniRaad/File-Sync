using FileSync.DomainModel.Enums;

namespace FileSync.DomainModel.Models
{
    public class SyncDirectory
    {
        public string SyncId { get; set; }
        public string Directory { get; set; }
        public RecursionType RecursionType { get; set; }
    }

}
