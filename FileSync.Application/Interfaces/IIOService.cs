namespace FileSync.Application.Interfaces
{
    public interface IIOService
    {
        string GetDirectorySettings();
        void SaveDirectorySettings(string syncDirectories);
    }
}