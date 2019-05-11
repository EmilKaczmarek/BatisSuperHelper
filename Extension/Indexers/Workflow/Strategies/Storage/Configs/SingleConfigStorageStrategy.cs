using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class SingleConfigStorageStrategy : IConfigStorageStrategy
    {
        private readonly IEnumerable<SqlMapConfig> _mapConfigs;
        private readonly IPackageStorage _storage;

        public SingleConfigStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs, IPackageStorage storage)
        {
            _mapConfigs = mapConfigs;
            _storage = storage;
        }

        public ConfigProcessingResult Store()
        {
            var config = _mapConfigs.First();
            return Store(config);
        }

        protected ConfigProcessingResult Store(SqlMapConfig config)
        {
            _storage.SqlMapConfigProvider.SetSingleMapConfig(config);

            return new ConfigProcessingResult
            {
                HasAtLeastOneProperConfig = true,
            };
        }
    }
}
