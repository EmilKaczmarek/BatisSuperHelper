using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Storage.Event
{
    public static class StorageEvents
    {
        public static event EventHandler<StoreChangeEventArgs> OnStoreChange;

        public static void TriggerStoreChange(object sender, StoreChangeEventArgs args)
        {
            OnStoreChange?.Invoke(sender, args);
        }
    }
}
