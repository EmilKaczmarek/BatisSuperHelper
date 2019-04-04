using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Constants
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
        public static readonly List<string> MethodNames = new List<string>
        {
            "QueryForObject",
            "QueryForList",
            "Delete",
            "Insert",           
            "Update",
        };

        public const string StatementsRootElementName = "statements";
        public const string StatementsRootElementXPath = "/sqlmap[1]/statements[1]";
        public const string StatmentIdAttributeName = "id";

        public const string MapFileRootElementName = "sqlMap";
        public const string MapFileRootElementXPath = "/sqlmap[1]";

        public const string SettingRootElementXPath = "/sqlmap[1]/settings[1]";
        public const string UseStatmentNameSpaceSettingAttributeName = "usestatementnamespaces";
    }
}
