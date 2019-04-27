﻿using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.Models.XmlConfig.SqlMap;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Parsers
{
    public class BatisXmlConfigParser : XmlParser
    {
        public SqlMapConfig Result => IsLazy ? _sqlMapConfigLazy.Value : GetFull();

        public Settings Settings => IsLazy ? _settingsLazy.Value : GetSetting();
        public IReadOnlyList<SqlMap> SqlMaps => IsLazy ? _sqlMapsLazy.Value : GetSqlMaps();

        private Lazy<Settings> _settingsLazy;
        private Lazy<IReadOnlyList<SqlMap>> _sqlMapsLazy;
        private Lazy<SqlMapConfig> _sqlMapConfigLazy;

        public BatisXmlConfigParser()
        {
            InitializeEmpty();
        }

        public BatisXmlConfigParser WithStringReader(StringReader stringReader)
        {
            InitializeWithStringReader(stringReader);
            return this;
        }

        public BatisXmlConfigParser WithFileInfo(string filePath, string fileProjectName)
        {
            InitializeWithFilePathAndProjectName(filePath, fileProjectName);
            return this;
        }

        public new BatisXmlConfigParser Load()
        {
            base.Load();
            return this;
        }

        public new void LazyLoading()
        {
            base.LazyLoading();
            _settingsLazy = new Lazy<Settings>(() => GetSetting());
            _sqlMapsLazy = new Lazy<IReadOnlyList<SqlMap>>(() => GetSqlMaps());
            _sqlMapConfigLazy = new Lazy<SqlMapConfig>(() => GetFull());
        }

        private SqlMapConfig GetFull()
        {
            return new SqlMapConfig
            {
                Settings = Settings,
                Maps = SqlMaps,
                ParsedSuccessfully = true,
            };
        }

        private Settings GetSetting()
        {
            var configSetting = new Settings();
            var settingNode = GetChildNodesOfParentByXPath(XmlConfigConstants.SettingXPath).Where(e => e.Name != "#text");
            foreach (var setting in settingNode)
            {
                if (setting.HasAttributes && setting.Attributes.Count == 1)
                {
                    var attribute = setting.Attributes.First();
                    configSetting.ApplyFromAttribute(attribute.Name, attribute.Value);
                }
            }
            return configSetting;
        }

        private IReadOnlyList<SqlMap> GetSqlMaps()
        {
            var sqlMaps = new List<SqlMap>();
            var sqlMapsNode = GetChildNodesOfParentByXPath(XmlConfigConstants.SqlMapXPath).Where(e => e.Name != "#text");
            foreach (var sqlMap in sqlMapsNode)
            {
                if (sqlMap.HasAttributes && sqlMap.Attributes.Count == 1)
                {
                    var attribute = sqlMap.Attributes.First();
                    sqlMaps.Add(SqlMap.FromAttribute(attribute.Name, attribute.Value));
                }
            }
            return sqlMaps;
        }
    }
}
