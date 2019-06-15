using EnvDTE;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Models;
using BatisSuperHelper.CoreAutomation.ProjectItems;

namespace BatisSuperHelper.Helpers
{
    public static class DocumentHelper
    {
        public static IEnumerable<XmlFileInfo> GetXmlFiles(IEnumerable<ProjectItem> projectItems)
        {
            return GetFilesByExtension(projectItems, ".xml");
        }

        public static IEnumerable<XmlFileInfo> GetXmlConfigFiles(IEnumerable<ProjectItem> projectItems)
        {
            return GetFilesByExtension(projectItems, ".config");
        }

        private static List<XmlFileInfo> GetFilesByExtension(IEnumerable<ProjectItem> projectItems, string extension)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var xmlFilesList = new List<XmlFileInfo>();

            bool needRefresh = false;
            foreach (var item in projectItems)
            {
                string fileName = null;
                try
                {
                    var hasCorrectExtension = item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile && item.FileCount == 1;
                    if (hasCorrectExtension)
                    {
                        fileName = item.FileNames[0];
                        if (Path.GetExtension(fileName).Equals(extension, StringComparison.CurrentCultureIgnoreCase))
                            xmlFilesList.Add(new XmlFileInfo
                            {
                                FilePath = (string)item.Properties.Item("FullPath").Value,
                                ProjectName = item.ContainingProject.Name,
                            });
                    }

                }
                catch (Exception)
                {
                    //Ignore item
                }
            }
            if (needRefresh)
            {
                var test = new ProjectItemRetreiver(GotoAsyncPackage.EnvDTE);
                return GetFilesByExtension(test.GetProjectItemsFromSolutionProjects(), extension);
            }
            return xmlFilesList;
        }
    }
}
