using System.Collections.Generic;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage;

namespace BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class FallbackConfigStrategy : IConfigStorageStrategy
    {
        private readonly IPackageStorage _storage;

        public FallbackConfigStrategy(IEnumerable<SqlMapConfig> mapConfigs, IPackageStorage storage)
        {
            _storage = storage;
        }

        public ConfigProcessingResult Store()
        {
            var defaultConfig = new SqlMapConfig();
            _storage.SqlMapConfigProvider.AddMultiple(new List<SqlMapConfig> { defaultConfig });

            return new ConfigProcessingResult
            {
                FallbackDefaultConfigUsed = true,
            };
        }
    }
}
