using BatisSuperHelper.Parsers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Indexers.Models
{
    public class XmlQuery : BaseIndexerValue
    {
        public string MapNamespace { get; set; }
        public string FullyQualifiedQuery { get; set; }
        public int? XmlLine { get; set; }
        public int? XmlLineColumn { get; set; }
        public SqlMapConfig Config => GotoAsyncPackage.Storage.SqlMapConfigProvider.GetConfigForMapFile(QueryFileName);
    }
}
