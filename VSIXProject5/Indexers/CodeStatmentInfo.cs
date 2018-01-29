using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Indexers
{
    public class CodeStatmentInfo
    {
        public int LineNumber { get; set; }
        public string StatmentFile { get; set; }
        public string RelativePath { get; set; }
        public string FilePath { get; set; }
    }
}
