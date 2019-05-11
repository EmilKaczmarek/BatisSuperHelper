using EnvDTE;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Config
{
    public class DefaultConfigStrategy : IConfigStrategy
    {
        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();
        private readonly ConfigsIndexingOptions _options;
        private readonly ConfigStorageStrategyFactory _strategyFactory;

        public DefaultConfigStrategy(ConfigsIndexingOptions options, ConfigStorageStrategyFactory strategyFactory)
        {
            _options = options;
            _strategyFactory = strategyFactory;
        }

        private IEnumerable<XmlFileInfo> GetConfigsFiles(IEnumerable<ProjectItem> projectItems)
        {
            return ConfigurationFilesHelper.GetBatisMapConfigFiles(DocumentHelper.GetXmlConfigFiles(projectItems));
        }

        private IEnumerable<SqlMapConfig> GetConfigs(IEnumerable<ProjectItem> projectItems)
        {
            var configs = GetConfigsFiles(projectItems);
            return configs.Select(config => _xmlIndexer.ParseSingleConfigFile(config)).Where(e => e.ParsedSuccessfully);
        }

        public ConfigProcessingResult Process(IEnumerable<ProjectItem> projectItems)
        {
            var configs = GetConfigs(projectItems);

            return _strategyFactory.GetStrategy(_options, configs).Store();
        }
    }
}
