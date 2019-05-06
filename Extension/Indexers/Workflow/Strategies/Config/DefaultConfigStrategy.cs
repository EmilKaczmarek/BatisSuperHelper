using EnvDTE;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers.Models;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Config
{
    public class DefaultConfigStrategy : IConfigStrategy
    {
        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();
        private readonly IEnumerable<ProjectItem> _projectItems;
        private readonly ConfigsIndexingOptions _options;
        private readonly ConfigStorageStrategyFactory _strategyFactory;

        public DefaultConfigStrategy(ConfigsIndexingOptions options, IEnumerable<ProjectItem> projectItems, ConfigStorageStrategyFactory strategyFactory)
        {
            _options = options;
            _projectItems = projectItems;
            _strategyFactory = strategyFactory;
        }

        private IEnumerable<XmlFileInfo> GetConfigsFiles()
        {
            return ConfigurationFilesHelper.GetBatisMapConfigFiles(DocumentHelper.GetXmlConfigFiles(_projectItems));
        }

        private IEnumerable<SqlMapConfig> GetConfigs()
        {
            var configs = GetConfigsFiles();
            return configs.Select(config => _xmlIndexer.ParseSingleConfigFile(config)).Where(e => e.ParsedSuccessfully);
        }

        public ConfigProcessingResult Process()
        {
            var configs = GetConfigs();

            return _strategyFactory.GetStrategy(_options, configs).Store();
        }
    }
}
