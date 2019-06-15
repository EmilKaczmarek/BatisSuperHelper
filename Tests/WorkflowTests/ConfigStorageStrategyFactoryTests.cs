using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class ConfigStorageStrategyFactoryTests
    {
        [Fact]
        public void ShouldGetSingleDefaultStrategyForEmptyList()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var parsedSuccessfully = Enumerable.Empty<SqlMapConfig>();

            var strategy = instance.GetStrategy(parsedSuccessfully);
            Assert.IsType<FallbackConfigStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetMultipleStrategyForMutlipeConfigsOptionAndListWithTwoDistinctItems()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);

            var parsedSuccessfully = new List<SqlMapConfig>
            {
                new SqlMapConfig
                {
                    Name = "Map1",
                },
                new SqlMapConfig
                {
                    Name = "Map2",
                }
            };

            var strategy = instance.GetStrategy(parsedSuccessfully);
            Assert.IsType<MultipleConfigsStorageStrategy>(strategy);
        }
    }
}
