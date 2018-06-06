using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Constants
{
    public static class IBatisConstants
    {
        public const string SqlMapNamespace = @"http://ibatis.apache.org/mapping";
        public static readonly List<string> StatementNames = new List<string>
        {
            "select",
            "insert",
            "delete",
            "update",
            "statement",
            "procedure",
            "sql"
        };

    }
}
