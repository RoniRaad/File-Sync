using System;

namespace FileSync.DomainModel.Models
{
    public class FSFileInfo
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
