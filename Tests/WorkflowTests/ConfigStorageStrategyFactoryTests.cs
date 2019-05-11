using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class ConfigStorageStrategyFactoryTests
    {
        [Fact]
        public void ShouldGetSingleDefaultStrategyForDefaultOptionsAndEmptyList()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions();
            var parsedSuccessfully = Enumerable.Empty<SqlMapConfig>();

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleDefaultConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleDefaultStrategyForNonDefaultOptionsAndEmptyList()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions
            {
                SupportMultipleConfigs = true,
            };
            var parsedSuccessfully = Enumerable.Empty<SqlMapConfig>();

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleDefaultConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleStrategyForDefaultOptionsAndListWithOneItem()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions();
            var parsedSuccessfully = new List<SqlMapConfig>
            {
                new SqlMapConfig
                {
                    Name = "Map1",
                },
            };

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleStrategyForDefaultOptionsAndListWithTwoDuplicatedItems()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions();

            var parsedSuccessfully = new List<SqlMapConfig>
            {
                new SqlMapConfig
                {
                    Name = "Map1",
                },
                new SqlMapConfig
                {
                    Name = "Map1",
                }
            };
            
            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleStrategyForMutlipeConfigsOptionAndListWithTwoDuplicatedItems()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions
            {
                SupportMultipleConfigs = true,
            };

            var parsedSuccessfully = new List<SqlMapConfig>
            {
                new SqlMapConfig
                {
                    Name = "Map1",
                },
                new SqlMapConfig
                {
                    Name = "Map1",
                }
            };

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleStrategyForMutlipeConfigsOptionAndListWithOneItem()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions
            {
                SupportMultipleConfigs = true,
            };

            var parsedSuccessfully = new List<SqlMapConfig>
            {
                new SqlMapConfig
                {
                    Name = "Map1",
                },
            };

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetMultipleStrategyForMutlipeConfigsOptionAndListWithTwoDistinctItems()
        {
            var packageStorageMock = new Mock<IPackageStorage>();
            var instance = new ConfigStorageStrategyFactory(packageStorageMock.Object);
            var options = new ConfigsIndexingOptions
            {
                SupportMultipleConfigs = true,
            };

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

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<MultipleConfigsStorageStrategy>(strategy);
        }
    }
}
