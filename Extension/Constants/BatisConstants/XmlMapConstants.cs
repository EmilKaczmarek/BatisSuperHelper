using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Constants.BatisConstants
{
    public static class XmlMapConstants
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

        public const string StatementsRootElementName = "statements";
        public const string StatementsRootElementXPath = "/sqlmap[1]/statements[1]";
        public const string StatmentIdAttributeName = "id";

        public const string MapFileRootElementName = "sqlMap";
        public const string MapFileRootElementXPath = "/sqlmap[1]";

    }
}
