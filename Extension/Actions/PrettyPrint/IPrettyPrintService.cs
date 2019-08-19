using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.PrettyPrint
{
    interface IPrettyPrintService
    {
        string PrettyPrint(string sql);
    }
}
