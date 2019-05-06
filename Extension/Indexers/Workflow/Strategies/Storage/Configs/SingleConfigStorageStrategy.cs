using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class SingleConfigStorageStrategy : IConfigStorageStrategy
    {
        private readonly IEnumerable<SqlMapConfig> _mapConfigs;

        public SingleConfigStorageStrategy(IEnumerable<SqlMapConfig> mapConfigs)
        {
            _mapConfigs = mapConfigs;
        }

        public ConfigProcessingResult Store()
        {
            var config = _mapConfigs.First();
            return Store(config);
        }

        protected ConfigProcessingResult Store(SqlMapConfig config)
        {
            GotoAsyncPackage.Storage.SetBatisSettings(config.Settings);

            return new ConfigProcessingResult
            {
                HasAtLeastOneProperConfig = true,
            };
        }
    }
}
