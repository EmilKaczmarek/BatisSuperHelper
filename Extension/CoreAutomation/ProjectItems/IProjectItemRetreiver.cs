using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.CoreAutomation.ProjectItems
{
    public interface IProjectItemRetreiver
    {
        IEnumerable<ProjectItem> GetProjectItemsFromSolutionProjects();

    }
}
