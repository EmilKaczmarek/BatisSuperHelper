﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BatisSuperHelper.Constants.BatisConstants;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Parsers.Models.XmlConfig.SqlMap;
using BatisSuperHelper.Parsers.XmlConfig.Models;
using Xunit;

namespace Tests
{
    public class BatisConfigParserTests
    {
        [Fact]
        public void SettingsFromProperConfig()
        {
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<sqlMapConfig xmlns=\"http://Batis.apache.org/dataMapper\"\r\nxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n\r\n  <settings>\r\n    <setting useStatementNamespaces=\"true\" />\r\n    <setting cacheModelsEnabled=\"true\" />\r\n    <setting validateSqlMap=\"true\" />\r\n  </settings>\r\n\r\n  <database>\r\n    <provider name=\"sqlServer\" />\r\n    <dataSource name=\"Store\" connectionString=\"\"/>\r\n  </database>\r\n\r\n  <sqlMaps>\r\n    <sqlMap embedded=\"sqlMap1.xml, CoolApp\" />\r\n  </sqlMaps>\r\n</sqlMapConfig>";
            var parser = new BatisXmlConfigParser().WithStringReader(new StringReader(content)).Load();

            var expectedSettings = new Settings
            {
                UseStatementNamespaces = true,
                CacheModelsEnabled = true,
                ValidateSqlMap = true,
                UseReflectionOptimizer = true,
            };

            Assert.Equal(expectedSettings, parser.Settings);
        }

        [Fact]
        public void SettingsFromNonValidConfig()
        {
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<sqlMapConfig xmlns=\"http://Batis.apache.org/dataMapper\"\r\nxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n\r\n  <setting\r\n    <setting useStatementNamespaces=\"true\" />\r\n    <setting cacheModelsEnabled=\"true\" />\r\n    <setting validateSqlMap=\"true\" />\r\n  </settings>\r\n\r\n  <database>\r\n    <provider name=\"sqlServer\" />\r\n    <dataSource name=\"Store\" connectionString=\"\"/>\r\n  </database>\r\n\r\n  <sqlMaps>\r\n    <sqlMap embedded=\"sqlMap1.xml, CoolApp\" />\r\n  </sqlMaps>\r\n</sqlMapConfig>";
            var parser = new BatisXmlConfigParser().WithStringReader(new StringReader(content)).Load();

            var expectedSettings = new Settings();

            Assert.Equal(expectedSettings, parser.Settings);
        }

        [Fact]
        public void SqlMapsFromProperConfig()
        {
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<sqlMapConfig xmlns=\"http://Batis.apache.org/dataMapper\"\r\nxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n\r\n  <setting\r\n    <setting useStatementNamespaces=\"true\" />\r\n    <setting cacheModelsEnabled=\"true\" />\r\n    <setting validateSqlMap=\"true\" />\r\n  </settings>\r\n\r\n  <database>\r\n    <provider name=\"sqlServer\" />\r\n    <dataSource name=\"Store\" connectionString=\"\"/>\r\n  </database>\r\n\r\n  <sqlMaps>\r\n    <sqlMap embedded=\"sqlMap1.xml, CoolApp\" />\r\n  </sqlMaps>\r\n</sqlMapConfig>";
            var parser = new BatisXmlConfigParser().WithStringReader(new StringReader(content)).Load();
            var expected = new List<SqlMapDefinition>
            {
                new SqlMapDefinition
                {
                    Value = "sqlMap1.xml",
                    RawValue = "sqlMap1.xml, CoolApp",
                    ResourceType = XmlConfigConstants.SqlMapResourceType.EMBEDDED,
                },
            };

            var actual = parser.SqlMaps.ToList();

            Assert.Single(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First(), actual.First());

            //CollectionAssert.AreEquivalent(expected, actual); //TODO: Move to nUnit or xUnit...
        }

        [Fact]
        public void SqlMapsFromNonValidConfig()
        {
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<sqlMapConfig xmlns=\"http://Batis.apache.org/dataMapper\"\r\nxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n\r\n  <settings>\r\n    <setting useStatementNamespaces=\"true\" />\r\n    <setting cacheModelsEnabled=\"true\" />\r\n    <setting validateSqlMap=\"true\" />\r\n  </settings>\r\n\r\n  <database>\r\n    <provider name=\"sqlServer\" />\r\n    <dataSource name=\"Store\" connectionString=\"\"/>\r\n  </database>\r\n\r\n  <sqlMaps>\r\n    <sqlMap embedded=\"sqlMap1.xml, CoolApp\" />\r\n  </sqlMaps>\r\n</sqlMapConfig>";
            var parser = new BatisXmlConfigParser().WithStringReader(new StringReader(content)).Load();
            var expected = new List<SqlMapDefinition>
            {
                new SqlMapDefinition
                {
                    Value = "sqlMap1.xml",
                    RawValue = "sqlMap1.xml, CoolApp",
                    ResourceType = XmlConfigConstants.SqlMapResourceType.EMBEDDED,
                },
            };

            var actual = parser.SqlMaps.ToList();

            Assert.Single(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First(), actual.First());

            //CollectionAssert.AreEquivalent(expected, actual); //TODO: Move to nUnit or xUnit...
        }

        [Fact]
        public void XmlNamespaceFromValidConfig()
        {
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<sqlMapConfig xmlns=\"http://Batis.apache.org/dataMapper\"\r\nxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n\r\n  <settings>\r\n    <setting useStatementNamespaces=\"true\" />\r\n    <setting cacheModelsEnabled=\"true\" />\r\n    <setting validateSqlMap=\"true\" />\r\n  </settings>\r\n\r\n  <database>\r\n    <provider name=\"sqlServer\" />\r\n    <dataSource name=\"Store\" connectionString=\"\"/>\r\n  </database>\r\n\r\n  <sqlMaps>\r\n    <sqlMap embedded=\"sqlMap1.xml, CoolApp\" />\r\n  </sqlMaps>\r\n</sqlMapConfig>";
            var parser = new BatisXmlConfigParser().WithStringReader(new StringReader(content)).Load();

            Assert.Equal(BatisXmlFileTypeEnum.SqlMapConfig, parser.BatisXmlFileType);
        }

    }
}
