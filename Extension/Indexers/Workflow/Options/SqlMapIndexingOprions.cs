using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Indexers.Workflow.Options
{
    public class SqlMapIndexingOptions
    {
        public bool IndexAllMaps { get; set; }
        public bool IndexOnlyMapsInConfig { get; set; }
        public bool MarkUnusedMap { get; set; }
        public bool IndexAllMapsOnError { get; set; }
    }
}
