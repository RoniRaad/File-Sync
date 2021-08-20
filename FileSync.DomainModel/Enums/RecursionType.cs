using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
