using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs
{
    public class ConfigStorageStrategyFactory
    {
        private readonly IPackageStorage _storage;

        public ConfigStorageStrategyFactory(IPackageStorage storage)
        {
            _storage = storage;
        }

        public IConfigStorageStrategy GetStrategy(IEnumerable<SqlMapConfig> successfullyParsed)
        {
            LogManager.GetLogger("error").Error($"Parsed: {string.Join(",", successfullyParsed.Select(e=>e.Name))}");
            if (successfullyParsed.Distinct().Any())
            {
                return new MultipleConfigsStorageStrategy(successfullyParsed, _storage);
            }

            return new FallbackConfigStrategy(successfullyParsed, _storage);
        }

    }
}
