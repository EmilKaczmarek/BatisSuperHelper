using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers.Workflow.Options
{
    public class SqlMapIndexingOprions
    {
        public bool IndexAllMaps { get; set; }
        public bool IndexOnlyMapsInConfig { get; set; }
        public bool MarkUnusedMap { get; set; }
    }
}
