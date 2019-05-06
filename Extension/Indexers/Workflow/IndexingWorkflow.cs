using EnvDTE;
using EnvDTE80;
using IBatisSuperHelper.CoreAutomation.ProjectItems;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Config;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using IBatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers.Workflow
{
    public class IndexingWorkflow
    {
        private readonly IndexingWorkflowOptions _options;

        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();

        private readonly IProjectItemRetreiver _projectItemRetreiver;
        private readonly IConfigStrategy _configStrategy;
        
        private IEnumerable<ProjectItem> _projectItems;

        public IndexingWorkflow(IndexingWorkflowOptions options, IProjectItemRetreiver projectItemRetreiver, IConfigStrategy configStrategy)
        {
            _options = options;
            _projectItemRetreiver = projectItemRetreiver;
            _configStrategy = configStrategy;
        }

        public void GetAndSetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projectItems = _projectItemRetreiver.GetProjectItemsFromSolutionProjects();
        }

        private MapProcessingResult HandleMaps(ConfigProcessingResult configProcessingResult)
        {
            var xmlFiles = DocumentHelper.GetXmlFiles(_projectItems);

            if (_options.MapsOptions.IndexAllMaps)
            {
                var indexingResult = _xmlIndexer.BuildIndexer(xmlFiles);
                PackageStorage.XmlQueries.AddMultipleWithoutKey(indexingResult);
                return new MapProcessingResult
                {
                    ProcessedFiles = indexingResult.Select(e => e.QueryFileName),
                };
            }

            if (_options.MapsOptions.IndexAllMaps && _options.MapsOptions.MarkUnusedMap)
            {
                var indexingResult = _xmlIndexer.BuildIndexer(xmlFiles);
                //TODO: Complete logic for marking unused maps.
                PackageStorage.XmlQueries.AddMultipleWithoutKey(indexingResult);
                return new MapProcessingResult
                {
                    ProcessedFiles = indexingResult.Select(e => e.QueryFileName),
                };
            }

            if (_options.MapsOptions.IndexOnlyMapsInConfig)
            {
                var indexingResult = _xmlIndexer.BuildIndexer(xmlFiles);
                //TODO: Complete logic for filtering unused maps.
                PackageStorage.XmlQueries.AddMultipleWithoutKey(indexingResult);
                return new MapProcessingResult
                {
                    ProcessedFiles = indexingResult.Select(e => e.QueryFileName),
                };
            }

            return new MapProcessingResult();
        }

        //Step 1: Config(s)
        //Step 2: Maps
        //Step 3: Code
        public void ExecuteIndexing()
        {
            var configResult = _configStrategy.Process();
            //var mapResults = HandleMaps(configResult);
        }
    }
}
