using EnvDTE;
using EnvDTE80;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Workflow.Options;
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
        private readonly DTE2 _dte;
        private readonly ProjectItemRetreiver _projectItemRetreiver = new ProjectItemRetreiver();
        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();

        private IReadOnlyList<ProjectItem> _projectItems;
        private IReadOnlyList<SqlMapConfig> _batisMapsConfigs;

        public IndexingWorkflow(IndexingWorkflowOptions options, DTE2 dte)
        {
            _options = options;
            _dte = dte;
        }

        public void GetAndSetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projectItems = _projectItemRetreiver.GetProjectItemsFromSolutionProjects(_dte.Solution.Projects);
        }

        private IEnumerable<XmlFileInfo> GetConfigsFiles()
        {
            return ConfigurationFilesHelper.GetBatisMapConfigFiles(DocumentHelper.GetXmlConfigFiles(_projectItems));
        }

        private void ApplySingleConfig(SqlMapConfig config)
        {
            PackageStorage.SetBatisSettings(config.Settings);
            //TODO: Apply aliases, providers etc to Store when implemented
        }

        private void ApplyMultipleConfigs(IEnumerable<SqlMapConfig> configs)
        {
            foreach (var config in configs)
            {
                //ApplySingleConfig(config);
            }
        }

        private ConfigProcessingResult HandleConfigs()
        {
            var configs = GetConfigsFiles();
            _batisMapsConfigs = configs.Select(config => _xmlIndexer.ParseSingleConfigFile(config)).Where(e=>e.ParsedSuccessfully).ToList();

            if (_options.ConfigOptions.SupportMultipleConfigs)
            {
                ApplyMultipleConfigs(_batisMapsConfigs);
                return ConfigProcessingResult.FromProcessingResults(_batisMapsConfigs);
            }

            if (!_options.ConfigOptions.SupportMultipleConfigs && configs.Count() > 1 && configs.Distinct().Count() == 1)
            {
                ApplySingleConfig(_batisMapsConfigs.First());
                return ConfigProcessingResult.FromProcessingResults(_batisMapsConfigs);
            }

            if (!configs.Any())
            {
                var defaultConfig = new SqlMapConfig();

                _batisMapsConfigs = new List<SqlMapConfig> { defaultConfig };

                ApplySingleConfig(defaultConfig);
                return ConfigProcessingResult.FallbackDefault;
            }

            ApplySingleConfig(_batisMapsConfigs.First());
            return ConfigProcessingResult.FallbackFirst;
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

        public void ExecuteIndexing()
        {
            var configResult = HandleConfigs();
            var mapResults = HandleMaps(configResult);

            //var xmlFiles = DocumentHelper.GetXmlFiles(projectItems);
            //var xmlIndexerResult = xmlIndexer.BuildIndexer(xmlFiles);
            //PackageStorage.XmlQueries.AddMultipleWithoutKey(xmlIndexerResult);
        }
    }
}
