using EnvDTE;
using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.CoreAutomation.ProjectItems
{
    public class ProjectItemRetreiver : IProjectItemRetreiver
    {
        private readonly List<ProjectItem> _projectItems = new List<ProjectItem>();
        private readonly int maxDepth = 10000;
        private int currentRecursiveCall = 1;

        private readonly DTE2 _dte;

        public ProjectItemRetreiver(DTE2 dte)
        {
            _dte = dte;
        }

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
        public IEnumerable<ProjectItem> GetProjectItemsFromSolutionProjects()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var projects = _dte.Solution.Projects;

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
