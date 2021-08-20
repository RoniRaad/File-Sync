using System;

namespace FileSync.DomainMode.Models
{
    public class FSFileInfo
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
