using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers.Models
{
    public class XmlQuery : BaseIndexerValue
    {
        public string MapNamespace { get; set; }
        public string FullyQualifiedQuery { get; set; }
        public bool UsesStatementNamespaces { get; set; }
    }
}
