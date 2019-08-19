using EnvDTE;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.Models;
using BatisSuperHelper.Parsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BatisSuperHelper.Loggers;

namespace BatisSuperHelper.Indexers.Workflow.Strategies.Config
{
    public class DefaultConfigStrategy : IConfigStrategy
    {
        private readonly XmlIndexer _xmlIndexer = new XmlIndexer();
        private readonly ConfigStorageStrategyFactory _strategyFactory;

        public DefaultConfigStrategy(ConfigStorageStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        private IEnumerable<XmlFileInfo> GetConfigsFiles(IEnumerable<ProjectItem> projectItems)
        {
            return ConfigurationFilesHelper.GetBatisMapConfigFiles(DocumentHelper.GetXmlConfigFiles(projectItems));
        }

        private IEnumerable<SqlMapConfig> GetConfigs(IEnumerable<ProjectItem> projectItems)
        {
            var configs = GetConfigsFiles(projectItems);
            OutputWindowLogger.WriteLn($"Found config candidates: {string.Join(" ", configs.Select(e=>e.FilePath))}");
            var parsedConfigs = configs.Select(config => _xmlIndexer.ParseSingleConfigFile(config)).Where(e => e.ParsedSuccessfully);
            OutputWindowLogger.WriteLn($"Parsed configs: {string.Join(" ", parsedConfigs.Select(e=>e.Name))}");
            return parsedConfigs;
        }

        public ConfigProcessingResult Process(IEnumerable<ProjectItem> projectItems)
        {
            var configs = GetConfigs(projectItems);

            return _strategyFactory.GetStrategy(configs).Store();
        }
    }
}
