using System;
using System.Collections.Generic;
using System.Linq;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Models;
using Xunit;

namespace Tests
{
    public class ConfigurationFilesHelperTests
    {
        [Fact]
        public void ForSingleKnowConfigFileName()
        {
            var configFiles = new List<XmlFileInfo>
            {
                new XmlFileInfo
                {
                    FilePath = "C:/Sources/AnotherImportantSolution/src/DAL/Configs/SqlMap.config",
                    ProjectName = "Test",
                },
                new XmlFileInfo
                {
                    FilePath = "C:/Sources/AnotherImportantSolution/src/app.config",
                    ProjectName = "Test",
                },
                new XmlFileInfo
                {
                    FilePath = "C:/Sources/AnotherImportantSolution/src/packages.config",
                    ProjectName = "Test",
                },
            };
            var sqlMapConfigs = ConfigurationFilesHelper.GetBatisMapConfigFiles(configFiles);

            Assert.True(sqlMapConfigs.Any());
            Assert.Single(sqlMapConfigs);
            Assert.Equal(configFiles.First(), sqlMapConfigs.First());
        }

        [Fact]
        public void ForNoConfigFileNames()
        {
            var configFiles = new List<XmlFileInfo>();

            var sqlMapConfigs = ConfigurationFilesHelper.GetBatisMapConfigFiles(configFiles);
            
            Assert.False(sqlMapConfigs.Any());
        }


        [Fact]
        public void ForNoKnownConfigFileNameAndOtherConfigs()
        {
            var configFiles = new List<XmlFileInfo> {
               new XmlFileInfo
                {
                    FilePath = "C:/Sources/AnotherImportantSolution/src/app.config",
                    ProjectName = "Test",
                },
                new XmlFileInfo
                {
                    FilePath = "C:/Sources/AnotherImportantSolution/src/packages.config",
                    ProjectName = "Test",
                },
            };

            var sqlMapConfigs = ConfigurationFilesHelper.GetBatisMapConfigFiles(configFiles);

            Assert.Empty(sqlMapConfigs);
        }

        //TODO: Write test for checking file that doesn't match sqlmap.config patterns, but are valid configs.

    }
}
