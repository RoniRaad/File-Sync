using System.Collections.Generic;

namespace FileSync
{
    public interface IIOService
    {
        IList<SyncDirectory> GetDirectorySettings();
        void SaveDirectorySettings(IList<SyncDirectory> syncDirectories);
    }
}