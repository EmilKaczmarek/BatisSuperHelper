using BatisSuperHelper.Storage.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Storage
{
    public class StoreChangeEventArgs : EventArgs
    {
        public ChangedFileTypeFlag ChangedFileType { get; private set; }

        public StoreChangeEventArgs(ChangedFileTypeFlag changedFileType )
        {
            ChangedFileType = changedFileType;
        }
    }
}
