using System.ComponentModel;

namespace FileSync.DomainModel.Enums
{
    public enum RecursionType
    {
        [Description("Recursive (All Files and Subfolders)")]
        Recursive,
        [Description("None")]
        None
    }
}
