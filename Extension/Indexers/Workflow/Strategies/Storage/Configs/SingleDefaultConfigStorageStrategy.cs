using System.Collections.Generic;
using IBatisSuperHelper.Parsers.Models;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class SingleDefaultConfigStorageStrategy : SingleConfigStorageStrategy
    {
        public SingleDefaultConfigStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs) : base(mapConfigs)
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
