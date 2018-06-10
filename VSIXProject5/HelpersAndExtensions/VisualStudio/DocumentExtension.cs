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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectItem">ProjectItem</param>
        /// <returns>EnvDTE Document, or null</returns>
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
        //Usable? no idea for better name.
        /// <summary>
        /// Retrieves Project Items that is parsable by indexers.
        /// Use when Project Item have only one document/file.
        /// </summary>
        /// <param name="projectItem">Project Item with single document</param>
        /// <returns>Env DTE Document, or null on exception.</returns>
        public static Document GetUsableDocumentFromProjectItemNonNested(this ProjectItem projectItem)
        {
            try
            {
                if (projectItem.Document == null)
                    return null;

                if (projectItem.Kind.Contains(VSConstants.ItemTypeGuid.PhysicalFolder_string))
                    return null;

                if (projectItem.Kind.Replace("-", "").Equals(VSConstants.ItemTypeGuid.PhysicalFolder_string.Replace("-", ""), StringComparison.OrdinalIgnoreCase))
                    return null;
                //ProjectItem kind - file representing project in solution.
                if (projectItem.Kind.Replace("-", "").Trim().Equals("66A26722 - 8FB5 - 11D2 - AA7E - 00C04F688DDE".Replace("-", "").Trim()))
                    return null;

                return projectItem.Document;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return null;
            }
        }
        /// <summary>
        /// Use when Project Item could have more than 1 document/file.
        /// </summary>
        /// <param name="projectItem"></param>
        /// <returns></returns>
        //public static List<XmlFileInfo> Get2SimpleProjectItemFromProjectItemNested(this ProjectItem projectItem)
        //{
        //    List<XmlFileInfo> simpleProjectItemsForProjectItem = new List<XmlFileInfo>();
            
        //    try
        //    {
        //        var projectItemsFileCount = projectItem.FileCount;
        //        var projectName = projectItem.ContainingProject.Name;
        //        for (int i = 0; i < projectItemsFileCount; i++)
        //        {
        //            try
        //            {
        //                var filePath = projectItem.FileNames[(short)i];
        //                Debug.WriteLine(filePath + " " + projectItem.Kind);
        //                XmlFileInfo simpleProjectItem = new XmlFileInfo
        //                {
        //                    FilePath = filePath,
        //                    ProjectName = projectName,
        //                    IsCSharpFile = Path.GetExtension(filePath) == ".cs",
        //                };
        //                simpleProjectItemsForProjectItem.Add(simpleProjectItem);
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine(ex.Message);
        //            }
        //        }
        //    }
        //    catch (COMException comException)
        //    {
        //        Debug.WriteLine(comException.Message);
        //    }
        //    return simpleProjectItemsForProjectItem;
        //}
        /// <summary>
        /// Get Simple Project Item List for list of ProjectItems(nested and non nested)
        /// </summary>
        /// <param name="projectItems"></param>
        /// <returns></returns>
        //public static List<XmlFileInfo> GetUsableSimpleProjectItemsFromProjectItemList(List<ProjectItem> projectItems)
        //{
        //    List<XmlFileInfo> simpleProjectItems = new List<XmlFileInfo>();

        //    foreach (var projectItem in projectItems)
        //    {
        //        if(projectItem.FileCount > 1)
        //        {
        //            Document document = projectItem.GetUsableDocumentFromProjectItemNonNested();

        //            if (document != null && document.Language == "CSharp")
        //            {
        //                XmlFileInfo simpleProjectItem = new XmlFileInfo
        //                {
        //                    FilePath = (string)projectItem.Properties.Item("FullPath").Value,
        //                    ProjectName = projectItem.ContainingProject.Name,
        //                    IsCSharpFile = true,
        //                };
        //                simpleProjectItems.Add(simpleProjectItem);
        //            }
        //        }
        //        else
        //        {
        //            simpleProjectItems.AddRange(projectItem.GetSimpleProjectItemFromProjectItemNested());
        //        }
        //    }
        //    return simpleProjectItems;
        //}

        public static List<XmlFileInfo> GetXmlFiles(List<ProjectItem> projectItems)
        {
            return projectItems
                .Where(x => x.FileCount == 1)
                .Where(x => Path.GetExtension(x.FileNames[0]).Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                .Select(x => new XmlFileInfo
                {
                    FilePath = (string)x.Properties.Item("FullPath").Value,
                    ProjectName = x.ContainingProject.Name,
                })
                .ToList();
        }
    }
}
