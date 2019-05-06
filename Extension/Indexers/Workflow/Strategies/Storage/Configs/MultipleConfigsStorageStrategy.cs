using System.Collections.Generic;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class MultipleConfigsStorageStrategy : IConfigStorageStrategy
    {
        private readonly IEnumerable<SqlMapConfig> _mapConfigs;

        public MultipleConfigsStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs)
        {
            _mapConfigs = mapConfigs;
        }

        public ConfigProcessingResult Store()
        {
            foreach (var config in _mapConfigs)
            {
                PackageStorage.SetBatisSettings(config.Settings);
            }

            return new ConfigProcessingResult
            {
                HasAtLeastOneProperConfig = true,
                HasMultipleProperConfigs = true,
            };
        }

    }
}
