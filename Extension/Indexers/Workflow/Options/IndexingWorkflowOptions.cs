using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers.Workflow.Options
{
    public class IndexingWorkflowOptions
    {
        public ConfigsIndexingOptions ConfigOptions { get; set; }
        public SqlMapIndexingOprions MapsOptions { get; set; }
    }
}
