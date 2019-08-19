using EnvDTE;
using EnvDTE80;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Indexers.Workflow.Strategies.Config;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.Models;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Parsers.Models.XmlConfig.SqlMap;
using BatisSuperHelper.Parsers.XmlConfig.Models;
using BatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Loggers;

namespace BatisSuperHelper.Indexers.Workflow
{
    public class IndexingWorkflow
    {
        private readonly IndexingWorkflowOptions _options;

        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();

        private readonly IProjectItemRetreiver _projectItemRetreiver;
        private readonly IConfigStrategy _configStrategy;

        private IEnumerable<ProjectItem> _projectItems = Enumerable.Empty<ProjectItem>();

        public IndexingWorkflow(IndexingWorkflowOptions options, IProjectItemRetreiver projectItemRetreiver, IPackageStorage storage)
        {
            _options = options;
            _projectItemRetreiver = projectItemRetreiver;
            _configStrategy = new DefaultConfigStrategy(new ConfigStorageStrategyFactory(storage));
        }

        public void GetAndSetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projectItems = _projectItemRetreiver.GetProjectItemsFromSolutionProjects();
        }


        private IEnumerable<XmlFileInfo> GetExistingInMaps(IEnumerable<XmlFileInfo> xmlFiles, IEnumerable<string> fileNames)
        {
            return xmlFiles.Where(e => fileNames.Contains(Path.GetFileName(e.FilePath)));
        }

        private MapProcessingResult HandleMaps(ConfigProcessingResult configProcessingResult)
        {
            var xmlFiles = DocumentHelper.GetXmlFiles(_projectItems);
            var configs = GotoAsyncPackage.Storage.SqlMapConfigProvider.GetAll();

            if (_options.MapsOptions.IndexAllMaps)
            {
                return HandleCorrectMapFiles(xmlFiles, configs);
            }

            if (_options.MapsOptions.IndexOnlyMapsInConfig)
            {
                var fileNames = configs.SelectMany(e => e.Maps).Select(e => e.Value);
                var filteredXmlFiles = GetExistingInMaps(xmlFiles, fileNames);

                if (fileNames.Any(e => e.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
                {
                    //Fallback scenario, when path/filename is invaild than just index all maps in solution,
                    //maps that are are not referenced in config, will be assigned to default config.
                    OutputWindowLogger.WriteLn("Found invaild character for file. Parsing all maps found in solution.");
                    return HandleAllMapFiles(xmlFiles, configs);
                }

                return HandleCorrectMapFiles(filteredXmlFiles, configs);
            }

            return new MapProcessingResult();
        }
        private IDictionary<SqlMapConfig, IEnumerable<XmlFileInfo>> CreateMapFileInfosPairs(IEnumerable<XmlFileInfo> xmlFiles, IEnumerable<SqlMapConfig> configs)
        {
            return configs.ToDictionary(config => config, config => xmlFiles.Where(e => config.Maps.Select(x => x.Value).Contains(Path.GetFileName(e.FilePath))));
        }

        private MapProcessingResult HandleCorrectMapFiles(IEnumerable<XmlFileInfo> filesInfos, IEnumerable<SqlMapConfig> configs)
        {
            var indexingResult = _xmlIndexer.BuildIndexer(CreateMapFileInfosPairs(filesInfos, configs));

            GotoAsyncPackage.Storage.XmlQueries.AddMultipleWithoutKey(indexingResult);

            return new MapProcessingResult
            {
                ProcessedFiles = indexingResult.Select(e => e.QueryFileName).ToList(),
            };
        }

        private MapProcessingResult HandleAllMapFiles(IEnumerable<XmlFileInfo> filesInfos, IEnumerable<SqlMapConfig> configs)
        {
            var mapFileInfosPair = CreateMapFileInfosPairs(filesInfos, configs);

            var unmatchedFileInfos = filesInfos.Except(mapFileInfosPair.Values.SelectMany(e => e)).ToList();
            if (unmatchedFileInfos.Any())
            {
                mapFileInfosPair.Add(new SqlMapConfig(), unmatchedFileInfos);
            }

            var indexingResult = _xmlIndexer.BuildIndexer(mapFileInfosPair);

            GotoAsyncPackage.Storage.XmlQueries.AddMultipleWithoutKey(indexingResult);

            return new MapProcessingResult
            {
                ProcessedFiles = indexingResult.Select(e => e.QueryFileName).ToList(),
            };
        }

        //Step 1: Config(s)
        //Step 2: Maps
        //Step 3: Code(todo, currently independant)
        public void ExecuteIndexing()
        {
            var configResult = _configStrategy.Process(_projectItems);
            var mapResults = HandleMaps(configResult);
        }
    }
}
