using IBatisSuperHelper.Indexers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Parsers.Models.SqlMap
{
    public class Statement : BaseIndexerValue
    {
        public string Id { get; set; }
        public string ParameterClass { get; set; }
        public string ResultClass { get; set; }
        public string ListClass { get; set; }
        public string ParameterMap { get; set; }
        public string ResultMap { get; set; }
        public string CacheModel { get; set; }
        public StatmentType Type { get; set; }

        public string MapNamespace { get; set; }
        public string FullyQualifiedQuery { get; set; }
        public int? XmlLine { get; set; }
        public int? XmlLineColumn { get; set; }

        public string Content { get; set; }
    }
}
