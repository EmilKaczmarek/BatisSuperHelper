using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Indexers
{
    public class StatmentInfo
    {
        public IndexerKey key { get; set; }
        public XmlStatmentInfo XmlInfo {get;set;}
        public CodeStatmentInfo CodeInfo { get; set; }
    }
}
