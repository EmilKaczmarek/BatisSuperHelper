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
using IBatisSuperHelper.Models;

namespace IBatisSuperHelper.Helpers
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

        public static List<ProjectItem> GetXmlProjectItems(List<ProjectItem> projectItems)
        {
            List<ProjectItem> items = new List<ProjectItem>();

            var filteredProjectItems = projectItems
               .Where(x => x.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
               .Where(x => x.FileCount == 1)
               .ToList();

            foreach (var item in filteredProjectItems)
            {
                try
                {
                    if (item.Name != null && item.Name.ToLower().Contains("xml"))
                    {
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return items;
        }

        public static List<XmlFileInfo> GetXmlFiles(List<ProjectItem> projectItems)
        {
            var xmlFilesList = new List<XmlFileInfo>();
            var filteredProjectItems = projectItems
                .Where(x => x.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                .Where(x => x.FileCount == 1)
                .ToList();
            foreach (var item in filteredProjectItems)
            {
                string fileName = null;
                try
                {
                    fileName = item.FileNames[0];
                    if (Path.GetExtension(fileName).Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                        xmlFilesList.Add(new XmlFileInfo
                        {
                            FilePath = (string)item.Properties.Item("FullPath").Value,
                            ProjectName = item.ContainingProject.Name,
                        });
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            return xmlFilesList;
        }
    }
}
