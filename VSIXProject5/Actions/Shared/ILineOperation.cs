using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Actions.Shared
{
    public interface ILineOperation
    {
        string GetQueryNameAtLine();
        bool CanRenameQueryAtLine();
    }
}
