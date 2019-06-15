using BatisSuperHelper;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Parsers.XmlConfig.Models;
using BatisSuperHelper.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class SingleDefaultConfigsStorageStrategyTests
    {
        [Fact]
        public void ShouldCallOneTime()
        {
            var defaultSetting = new SqlMapConfig();

            var storageMock = new Mock<IPackageStorage>();
            storageMock.Setup(e => e.SqlMapConfigProvider.AddMultiple(It.IsAny<IEnumerable<SqlMapConfig>>()));

            var instance = new FallbackConfigStrategy(Enumerable.Empty<SqlMapConfig>(), storageMock.Object);
            instance.Store();

            storageMock.Verify(e => e.SqlMapConfigProvider.AddMultiple(new List<SqlMapConfig> { defaultSetting }));
        }
    }
}
