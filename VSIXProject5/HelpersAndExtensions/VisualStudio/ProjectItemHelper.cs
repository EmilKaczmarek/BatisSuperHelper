using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Helpers
{
    public class ProjectItemHelper
    {
        private readonly List<ProjectItem> _projectItems = new List<ProjectItem>();
        private readonly int maxDepth = 10000;
        private int currentRecursiveCall = 1;

        private ProjectItem GetFiles(ProjectItem item)
        {
            if (item.ProjectItems == null)
                return item;

            if (currentRecursiveCall > maxDepth)
                return item;

            currentRecursiveCall++;

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                _projectItems.Add(GetFiles(currentItem));
            }

            return item;
        }

        public List<ProjectItem> GetProjectItemsFromSolutionProjects(EnvDTE.Projects projects)
        {
            var enumerator = projects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var project = enumerator.Current as Project;
                if (project != null && project.ProjectItems != null)
                {
                    var items = project.ProjectItems.GetEnumerator();
                    while (items.MoveNext())
                    {
                        var item = (ProjectItem)items.Current;
                        _projectItems.Add(GetFiles(item));
                    }
                }
            }
            return _projectItems;
        }
    }
}
