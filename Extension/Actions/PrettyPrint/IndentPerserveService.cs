using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.PrettyPrint
{
    public class IndentPerserveService
    {
        private readonly string _originalSql;
        private readonly string _formattedSql;

        public IndentPerserveService(string originalSql, string formattedSql)
        {
            _originalSql = originalSql;
            _formattedSql = formattedSql;
        }

        public string GetFormattedSqlWithOriginalIndents()
        {
            var originalLines = _originalSql.Split(new[]{Environment.NewLine}, StringSplitOptions.None);
            var formattedLines = _formattedSql.Split(new[]{Environment.NewLine}, StringSplitOptions.None);

            if (originalLines.Any() && formattedLines.Any())
            {
                var whitespaces = string.Concat(originalLines[0].TakeWhile(e => char.IsWhiteSpace(e)));
                StringBuilder reformattedSb = new StringBuilder();
                foreach (var formattedLine in formattedLines)
                {
                    reformattedSb.AppendLine($"{whitespaces}{formattedLine}");
                }
                return reformattedSb.ToString().TrimEnd();
            }
            
            return null;
        }


    }
}
