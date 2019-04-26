using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Constants.BatisConstants;
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
    public class BatisXmlConfigParser
    {
        public Settings Setting => GetSetting();
        public IReadOnlyList<SqlMap> SqlMaps => GetSqlMaps();

        private XmlParser _parser;

        public BatisXmlConfigParser()
        {
            _parser = new XmlParser();
        }

        public BatisXmlConfigParser WithStringReader(StringReader stringReader)
        {
            _parser = new XmlParser(stringReader);
            return this;
        }

        public BatisXmlConfigParser WithFileInfo(string filePath, string fileProjectName)
        {
            _parser = new XmlParser(filePath, fileProjectName);
            return this;
        }

        public new BatisXmlConfigParser Load()
        {
            _parser.Load();
            return this;
        }

        private Settings GetSetting()
        {
            var configSetting = new Settings();
            var settingNode = _parser.GetChildNodesOfParentByXPath(XmlConfigConstants.SettingXPath).Where(e => e.Name != "#text");
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
            var sqlMapsNode = _parser.GetChildNodesOfParentByXPath(XmlConfigConstants.SqlMapXPath).Where(e => e.Name != "#text");
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
