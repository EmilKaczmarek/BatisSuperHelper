using System.Collections.Generic;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class SingleDefaultConfigStorageStrategy : SingleConfigStorageStrategy
    {
        public SingleDefaultConfigStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs, IPackageStorage storage) : base(mapConfigs, storage)
        {
        }

        public new ConfigProcessingResult Store()
        {
            var defaultConfig = new SqlMapConfig();
            Store(defaultConfig);

            return new ConfigProcessingResult
            {
                FallbackDefaultConfigUsed = true,
            };
        }
    }
}
