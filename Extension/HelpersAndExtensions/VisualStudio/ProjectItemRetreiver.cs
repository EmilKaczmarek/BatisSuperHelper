using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Helpers
{
    public class ProjectItemRetreiver
    {
        private readonly List<ProjectItem> _projectItems = new List<ProjectItem>();
        private readonly int maxDepth = 10000;
        private int currentRecursiveCall = 1;

        /// <summary>
        /// Get file from Project Item, recursive if needed.
        /// </summary>
        /// <param name="projectItem"></param>
        /// <returns></returns>
        private ProjectItem GetFiles(ProjectItem projectItem)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.ProjectItems == null)
                return projectItem;

            if (currentRecursiveCall > maxDepth)
                return projectItem;

            currentRecursiveCall++;

            var items = projectItem.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                _projectItems.Add(GetFiles(currentItem));
            }
            return projectItem;
        }
        /// <summary>
        /// Get all ProjectItems for given projects.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<ProjectItem> GetProjectItemsFromSolutionProjects(Projects projects)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var enumerator = projects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is Project project && project.ProjectItems != null)
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
