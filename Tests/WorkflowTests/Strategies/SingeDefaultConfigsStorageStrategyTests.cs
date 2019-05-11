using IBatisSuperHelper;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using IBatisSuperHelper.Storage;
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

            var instance = new SingleDefaultConfigStorageStrategy(Enumerable.Empty<SqlMapConfig>(), storageMock.Object);
            instance.Store();

            storageMock.Verify(e => e.SqlMapConfigProvider.SetSingleMapConfig(defaultSetting));
        }
    }
}
