using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;

namespace VSIXProject5.EventHandlers
{
    public class ProjectItemEventsEx
    {
        public void ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            Indexer.RenameStatmentsFile(OldName, ProjectItem.Name);
        }

        public void ItemRemoved(ProjectItem ProjectItem)
        {
            string fileName = ProjectItem.Name;
            Indexer.RemoveStatmentsForFile(fileName, false);
        }

        public void ItemAdded(ProjectItem ProjectItem)
        {
            string projectItemExtension = Path.GetExtension(ProjectItem.Name);
            if (projectItemExtension == ".xml")
            {
                XmlIndexer indexerInstance = new XmlIndexer();
                var xmlIndexer = indexerInstance.BuildUsingFilePath(ProjectItem.FileNames[0]);
                Indexer.Build(xmlIndexer);
            }
        }
    }
}
