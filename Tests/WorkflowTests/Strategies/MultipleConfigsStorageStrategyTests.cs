using BatisSuperHelper;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class MultipleConfigsStorageStrategyTests
    {
        [Fact]
        public void ShouldCallMultipleTimes()
        {
            var map1 = new SqlMapConfig();
            var map2 = new SqlMapConfig();

            List<SqlMapConfig> maps = new List<SqlMapConfig>
            {
                map1, map2
            };
            var storageMock = new Mock<IPackageStorage>();
            storageMock.Setup(e => e.SqlMapConfigProvider.AddMultiple(It.IsAny<IEnumerable<SqlMapConfig>>()));

            var instance = new MultipleConfigsStorageStrategy(maps, storageMock.Object);
            instance.Store();

            storageMock.Verify(e => e.SqlMapConfigProvider.AddMultiple(maps));
        }
    }
}
