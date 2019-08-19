using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.ActionValidators
{
    public interface IActionValidator
    {
        bool CanJumpToQueryInLine(int lineNumber);
        bool CanRenameQueryInLine(int lineNumber);
    }
}
