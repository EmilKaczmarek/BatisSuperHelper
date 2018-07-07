using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;
using VSIXProject5.Parsers;

namespace VSIXProject5.EventHandlers
{
    public class ProjectItemEventsEx
    {
        public void ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            Indexer.Instance.RenameStatmentsFile(OldName, ProjectItem.Name);
        }

        public void ItemRemoved(ProjectItem ProjectItem)
        {
            string fileName = ProjectItem.Name;
            Indexer.Instance.RemoveStatmentsForFile(fileName, false);
        }

        public void ItemAdded(ProjectItem ProjectItem)
        {
            string projectItemExtension = Path.GetExtension(ProjectItem.Name);
            if (projectItemExtension == ".xml")
            {
                XmlParser parser = XmlParser.WithFilePathAndFileInfo(ProjectItem.FileNames[0], ProjectItem.ContainingProject.Name);
                Indexer.Instance.Build(parser.GetMapFileStatments());
            }
        }
    }
}
