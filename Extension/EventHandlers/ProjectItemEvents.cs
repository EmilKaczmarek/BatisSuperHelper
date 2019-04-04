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
using NLog;
using IBatisSuperHelper.Loggers;

namespace IBatisSuperHelper.EventHandlers
{
    public class ProjectItemEventsEx
    {
        public void ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            try
            {
                PackageStorage.CodeQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
                PackageStorage.XmlQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "ProjectItemEventsEx.ItemRenamed");
                OutputWindowLogger.WriteLn($"Exception occured during ProjectItemEventsEx.ItemRenamed: { ex.Message}");
            }
        }

        public void ItemRemoved(ProjectItem ProjectItem)
        {
            try
            {
                string fileName = ProjectItem.Name;
                PackageStorage.XmlQueries.RemoveStatmentsForFilePath(fileName);
                PackageStorage.CodeQueries.RemoveStatmentsForFilePath(fileName);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "ProjectItemEventsEx.ItemRemoved");
                OutputWindowLogger.WriteLn($"Exception occured during ProjectItemEventsEx.ItemRemoved: { ex.Message}");
            }
        }

        public void ItemAdded(ProjectItem ProjectItem)
        {
            try
            {
                string projectItemExtension = Path.GetExtension(ProjectItem.Name);
                if (projectItemExtension == ".xml")
                {
                    PackageStorage.AnalyzeAndStoreSingle(new XmlFileInfo
                    {
                        FilePath = ProjectItem.FileNames[0],
                        ProjectName = ProjectItem.ContainingProject.Name
                    });
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "ProjectItemEventsEx.ItemAdded");
                OutputWindowLogger.WriteLn($"Exception occured during ProjectItemEventsEx.ItemAdded: { ex.Message}");
            }
        }
    }
}
