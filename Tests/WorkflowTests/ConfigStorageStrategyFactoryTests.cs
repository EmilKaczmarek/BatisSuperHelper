using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Parsers.Models;
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
            var instance = new ConfigStorageStrategyFactory();
            var options = new ConfigsIndexingOptions();
            var parsedSuccessfully = Enumerable.Empty<SqlMapConfig>();

            var strategy = instance.GetStrategy(options, parsedSuccessfully);
            Assert.IsType<SingleDefaultConfigStorageStrategy>(strategy);
        }

        [Fact]
        public void ShouldGetSingleDefaultStrategyForNonDefaultOptionsAndEmptyList()
        {
            var instance = new ConfigStorageStrategyFactory();
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
            var instance = new ConfigStorageStrategyFactory();
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
            var instance = new ConfigStorageStrategyFactory();
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
            var instance = new ConfigStorageStrategyFactory();
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
            var instance = new ConfigStorageStrategyFactory();
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
            var instance = new ConfigStorageStrategyFactory();
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
