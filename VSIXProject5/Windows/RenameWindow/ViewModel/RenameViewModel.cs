using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Windows.RenameWindow.ViewModel
{
    public class RenameViewModel
    {
        public bool IsMethodRenameChecked { get; set; }
        public string QueryText { get; set; }
        public bool WasInputCanceled { get; set; }

        public RenameViewModel(bool isMethodRenameChecked, string queryText)
        {
            IsMethodRenameChecked = isMethodRenameChecked;
            QueryText = queryText;
        }

        public RenameViewModel()
        {
        }
    }
}
