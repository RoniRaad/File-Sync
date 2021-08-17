using System.Collections.Generic;

namespace FileSync
{
    public interface IIOService
    {
        string GetDirectorySettings();
        void SaveDirectorySettings(string syncDirectories);
    }
}