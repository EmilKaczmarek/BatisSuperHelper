using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Actions2.ActionValidators
{
    public interface IActionValidator
    {
        bool CanJumpToQueryInLine(int lineNumber);
        bool CanRenameQueryInLin(int lineNumber);
    }
}
