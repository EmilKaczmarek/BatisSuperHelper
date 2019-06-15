using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Models;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Storage;
using NLog;
using BatisSuperHelper.Loggers;

namespace BatisSuperHelper.EventHandlers
{
    public class ProjectItemEventsActions
    {
        public void ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            try
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                GotoAsyncPackage.Storage.CodeQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
                GotoAsyncPackage.Storage.XmlQueries.RenameStatmentsForFile(OldName, ProjectItem.Name);
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
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                string fileName = ProjectItem.Name;
                GotoAsyncPackage.Storage.XmlQueries.RemoveStatmentsForFilePath(fileName);
                GotoAsyncPackage.Storage.CodeQueries.RemoveStatmentsForFilePath(fileName);
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
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                string projectItemExtension = Path.GetExtension(ProjectItem.Name);
                if (projectItemExtension == ".xml")
                {
                    GotoAsyncPackage.Storage.AnalyzeAndStoreSingle(new XmlFileInfo
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
