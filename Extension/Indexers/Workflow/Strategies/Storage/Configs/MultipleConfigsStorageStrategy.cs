using System.Collections.Generic;
using System.Linq;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
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

        private SqlMapConfig DetermineCurrentConfig()
        {
            var configWithKnownName = _mapConfigs.FirstOrDefault(e => e.Name == XmlConfigConstants.KnowFileName);
            if (configWithKnownName != null)
            {
                return configWithKnownName;
            }

            return _mapConfigs.First();
        }

        public ConfigProcessingResult Store()
        {
            _storage.SqlMapConfigProvider.SetMultipleMapConfigs(_mapConfigs, DetermineCurrentConfig());

            return new ConfigProcessingResult
            {
                HasAtLeastOneProperConfig = true,
                HasMultipleProperConfigs = true,
            };
        }

    }
}
