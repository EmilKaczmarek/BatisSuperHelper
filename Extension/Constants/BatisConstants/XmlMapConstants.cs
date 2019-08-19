using System.Collections.Generic;

namespace BatisSuperHelper.Constants.BatisConstants
{
#pragma warning disable S1075 // URIs should not be hardcoded
    public static class XmlMapConstants
    {
        public const string XmlNamespace = @"http://ibatis.apache.org/mapping";

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

        public const string RootElementName = "sqlMap";
        public const string MapFileRootElementXPath = "/sqlmap[1]";

    }
}
#pragma warning restore S1075 // URIs should not be hardcoded
