using EnvDTE80;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.CoreAutomation.ProjectItems;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.EventHandlers.SolutionEventsActions
{
    public class VSSolutionEventsActions : IVSSolutionEventsActions
    {
        private readonly DTE2 _dte;
        private readonly XmlIndexer _xmlIndexer;

        public VSSolutionEventsActions(DTE2 dte, XmlIndexer indexer)
        {
            _dte = dte;
            _xmlIndexer = indexer;
        }

        public void OnSolutionLoadComplete()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectItemHelper = new ProjectItemRetreiver(_dte);
            var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects();
            XmlIndexer xmlIndexer = new XmlIndexer();

            var configFiles = ConfigurationFilesHelper.GetBatisMapConfigFiles(DocumentHelper.GetXmlConfigFiles(projectItems));
            if (configFiles.Any())
            {
                PackageStorage.SetBatisSettings(_xmlIndexer.ParseSingleConfigFile(configFiles.First()).Settings);
            }

            var xmlFiles = DocumentHelper.GetXmlFiles(projectItems);
            var xmlIndexerResult = xmlIndexer.BuildIndexer(xmlFiles);
            PackageStorage.XmlQueries.AddMultipleWithoutKey(xmlIndexerResult);
        }

        public void SolutionOnClose()
        {
            PackageStorage.CodeQueries.Clear();
            PackageStorage.XmlQueries.Clear();
        }
    }
}
