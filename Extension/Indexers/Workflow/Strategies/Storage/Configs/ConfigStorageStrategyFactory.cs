using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Parsers.Models;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class ConfigStorageStrategyFactory
    {
        public IConfigStorageStrategy GetStrategy(ConfigsIndexingOptions options, IEnumerable<SqlMapConfig> successfullyParsed)
        {

            if (options.SupportMultipleConfigs && successfullyParsed.Distinct().Count() > 1)
            {
                return new MultipleConfigsStorageStrategy(successfullyParsed);
            }

            if (successfullyParsed.Distinct().Count() == 1)
            {
                return new SingleConfigStorageStrategy(successfullyParsed);
            }

            return new SingleDefaultConfigStorageStrategy(successfullyParsed);
        }

    }
}
