using PoorMansTSqlFormatterLib.Formatters;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.PrettyPrint
{
    public class PrettyPrintService : IPrettyPrintService
    {
        private readonly TSqlStandardFormatter _formatter;
        public PrettyPrintService()
        {
            _formatter = new TSqlStandardFormatter
            {
                TrailingCommas = true
            };
        }

        public string PrettyPrint(string sql)
        {
            var tokenizer = new TSqlStandardTokenizer();
            var tokenized = tokenizer.TokenizeSQL(sql);
            var parser = new TSqlStandardParser();
            var parsedSql = parser.ParseSQL(tokenized);

            return _formatter.FormatSQLTree(parsedSql)?.TrimEnd();
        }
    }
}
