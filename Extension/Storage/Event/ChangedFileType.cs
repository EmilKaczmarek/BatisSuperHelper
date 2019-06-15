using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Storage.Event
{
    [Flags]
    public enum ChangedFileTypeFlag
    {
        Xml = 1,
        CSharp = 2,
    }

    public static class ChangedFileTypeExtensions
    {
        public static bool IsSet(this ChangedFileTypeFlag flags, ChangedFileTypeFlag flag)
        {
            return ((int)flags & (int)flag) != 0;
        }
    }
}
