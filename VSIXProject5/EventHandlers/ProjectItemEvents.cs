using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.EventHandlers
{
    public class ProjectItemEventsEx
    {
        public void ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            PackageStorage.CodeQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
            PackageStorage.XmlQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
        }

        public void ItemRemoved(ProjectItem ProjectItem)
        {
            string fileName = ProjectItem.Name;
            PackageStorage.XmlQueries.RemoveStatmentsForFilePath(fileName);
            PackageStorage.CodeQueries.RemoveStatmentsForFilePath(fileName);
        }

        public void ItemAdded(ProjectItem ProjectItem)
        {
            string projectItemExtension = Path.GetExtension(ProjectItem.Name);
            if (projectItemExtension == ".xml")
            {
                PackageStorage.AnalyzeAndStoreSingle(new XmlFileInfo
                {
                    FilePath = ProjectItem.FileNames[0],
                    ProjectName =  ProjectItem.ContainingProject.Name
                });
            }
        }
    }
}
