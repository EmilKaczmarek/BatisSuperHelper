using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Indexers.Workflow.Strategies.Config
{
    public interface IConfigStrategy
    {
        ConfigProcessingResult Process(IEnumerable<ProjectItem> projectItems);
    }
}
