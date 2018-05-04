using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions
{
    public static class XmlHelper
    {
        public static class XmlStringLine
        {
            public static bool IsBlank(string line)
            {
                return line.Trim() == "";
            }

            public static bool IsCommentBegin(string line)
            {
                return line.TrimStart().StartsWith("<!--", StringComparison.Ordinal);
            }

            public static bool IsCommentEnd(string line)
            {
                return line.Trim() == "-->";
            }

            public static bool IsIgnored(string line)
            {
                return IsBlank(line) || IsCommentBegin(line) || IsCommentEnd(line);
            }
        }
    }
}
