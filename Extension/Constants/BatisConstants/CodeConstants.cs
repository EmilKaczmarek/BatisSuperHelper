using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Constants.BatisConstants
{
    public static class CodeConstants
    {
        public static readonly List<string> MethodNames = new List<string>
        {
            "QueryForObject",
            "QueryForList",
            "Delete",
            "Insert",           
            "Update",
        };
    }
}
