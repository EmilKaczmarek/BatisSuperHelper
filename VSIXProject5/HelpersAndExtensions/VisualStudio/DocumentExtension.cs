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
using VSIXProject5.Models;

namespace VSIXProject5.Helpers
{
    public static class DocumentHelper
    {
        public static EnvDTE.Document GetDocumentOrDefault(this ProjectItem projectItem)
        {
            try
            {
                return projectItem.Document;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return null;
            }
        }

        public static Document GetDocumentFromProjectItemNonNested(this ProjectItem projectItem)
        {
            try
            {
                if (projectItem.Document == null)
                    return null;

                if (projectItem.Kind.Contains(VSConstants.ItemTypeGuid.PhysicalFolder_string))
                    return null;

                if (projectItem.Kind.Replace("-", "").Equals(VSConstants.ItemTypeGuid.PhysicalFolder_string.Replace("-", ""), StringComparison.OrdinalIgnoreCase))
                    return null;

                return projectItem.Document;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return null;
            }
        }

        public static List<SimpleProjectItem> GetSimpleProjectItemFromProjectItemNested(this ProjectItem projectItem)
        {
            List<SimpleProjectItem> simpleProjectItemsForProjectItem = new List<SimpleProjectItem>();
            
            try
            {
                var projectItemsFileCount = projectItem.FileCount;
                var projectName = projectItem.ContainingProject.Name;
                for (int i = 0; i < projectItemsFileCount; i++)
                {
                    try
                    {
                        var filePath = projectItem.FileNames[(short)i];
                        SimpleProjectItem simpleProjectItem = new SimpleProjectItem
                        {
                            FilePath = filePath,
                            ProjectName = projectName,
                            IsCSharpFile = Path.GetExtension(filePath) == ".cs",
                        };
                        simpleProjectItemsForProjectItem.Add(simpleProjectItem);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        System.Diagnostics.Debugger.Break();
                    }
                }
            }
            catch (COMException comException)
            {
                Debug.WriteLine(comException.Message);
                //System.Diagnostics.Debugger.Break();
            }
            return simpleProjectItemsForProjectItem;
        }

        public static List<SimpleProjectItem> GetDocumentsFromProjectItemList(List<ProjectItem> projectItems)
        {
            List<SimpleProjectItem> simpleProjectItems = new List<SimpleProjectItem>();

            foreach (var projectItem in projectItems)
            {
                if(projectItem.FileCount > 1)
                {
                    Document document = projectItem.GetDocumentFromProjectItemNonNested();

                    if (document != null && document.Language == "CSharp")
                    {
                        SimpleProjectItem simpleProjectItem = new SimpleProjectItem
                        {
                            FilePath = (string)projectItem.Properties.Item("FullPath").Value,
                            ProjectName = projectItem.ContainingProject.Name,
                            IsCSharpFile = true,
                        };
                        simpleProjectItems.Add(simpleProjectItem);
                    }
                }
                else
                {
                    //This is alternative approach for getting document...
                    simpleProjectItems.AddRange(projectItem.GetSimpleProjectItemFromProjectItemNested());
                }
            }
            return simpleProjectItems;
        }


    }
}
