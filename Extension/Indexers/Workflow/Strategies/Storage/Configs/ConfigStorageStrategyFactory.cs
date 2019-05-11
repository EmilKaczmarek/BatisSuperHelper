using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class ConfigStorageStrategyFactory
    {
        private readonly IPackageStorage _storage;

        public ConfigStorageStrategyFactory(IPackageStorage storage)
        {
            _storage = storage;
        }

        public IConfigStorageStrategy GetStrategy(ConfigsIndexingOptions options, IEnumerable<SqlMapConfig> successfullyParsed)
        {

            if (options.SupportMultipleConfigs && successfullyParsed.Distinct().Count() > 1)
            {
                return new MultipleConfigsStorageStrategy(successfullyParsed, _storage);
            }

            if (successfullyParsed.Distinct().Count() == 1)
            {
                return new SingleConfigStorageStrategy(successfullyParsed, _storage);
            }

            return new SingleDefaultConfigStorageStrategy(successfullyParsed, _storage);
        }

    }
}
