using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Constants;

namespace VSIXProject5.Helpers
{
    public static class IBatisHelper
    {
        public static bool IsIBatisStatment(string statment)
        {
            return IBatisConstants.StatementNames.Any(e => e == statment);
        }
    }
}
