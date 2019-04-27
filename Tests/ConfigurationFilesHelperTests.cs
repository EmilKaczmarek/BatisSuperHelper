using System;
using System.Collections.Generic;
using System.Linq;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ConfigurationFilesHelperTests
    {
        [TestMethod]
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

            Assert.IsTrue(sqlMapConfigs.Any());
            Assert.AreEqual(1, sqlMapConfigs.Count);
            Assert.AreEqual(configFiles.First(), sqlMapConfigs.First());
        }

        [TestMethod]
        public void ForNoConfigFileNames()
        {
            var configFiles = new List<XmlFileInfo>();

            var sqlMapConfigs = ConfigurationFilesHelper.GetBatisMapConfigFiles(configFiles);
            
            Assert.IsFalse(sqlMapConfigs.Any());
        }


        [TestMethod]
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

            Assert.IsFalse(sqlMapConfigs.Any());
        }

        //TODO: Write test for checking file that doesn't match sqlmap.config patterns, but are valid configs.

    }
}
