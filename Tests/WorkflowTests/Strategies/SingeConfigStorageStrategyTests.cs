using IBatisSuperHelper;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class SingleConfigStorageStrategyTests
    {
        [Fact]
        public void ShouldCallOneTime()
        {
            var map = new SqlMapConfig();

            List<SqlMapConfig> maps = new List<SqlMapConfig>
            {
                map
            };

            var storageMock = new Mock<IPackageStorage>();

            var instance = new SingleConfigStorageStrategy(maps, storageMock.Object);
            instance.Store();

            storageMock.Verify(e => e.SqlMapConfigProvider.SetSingleMapConfig(map));
        }
    }
}
