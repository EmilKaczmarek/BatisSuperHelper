using System.Collections.Generic;
using System.Linq;
using BatisSuperHelper.Constants.BatisConstants;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage;

namespace BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class MultipleConfigsStorageStrategy : IConfigStorageStrategy
    {
        private readonly IEnumerable<SqlMapConfig> _mapConfigs;
        private readonly IPackageStorage _storage;

        public MultipleConfigsStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs, IPackageStorage storage)
        {
            _mapConfigs = mapConfigs;
            _storage = storage;
        }

        public ConfigProcessingResult Store()
        {
            _storage.SqlMapConfigProvider.AddMultiple(_mapConfigs);

            return new ConfigProcessingResult
            {
                HasAtLeastOneProperConfig = true,
                HasMultipleProperConfigs = true,
            };
        }

    }
}
