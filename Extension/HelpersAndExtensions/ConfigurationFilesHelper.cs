﻿using BatisSuperHelper.Constants.BatisConstants;
using BatisSuperHelper.Models;
using BatisSuperHelper.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BatisSuperHelper.HelpersAndExtensions
{
    public static class ConfigurationFilesHelper
    {
        private static readonly IReadOnlyList<string> DotNetCommonConfigNames = new List<string>
        {
            "packages.config",
            "app.config",
        };
    

        public static List<XmlFileInfo> GetBatisMapConfigFiles(IEnumerable<XmlFileInfo> allConfigFiles)
        {
            var sqlMapConfigs = FindConfigsWithKnowFileName(allConfigFiles);
            if (sqlMapConfigs.Any())
            {
                return sqlMapConfigs;
            }

            var otherConfigs = FilterDotNetCommonConfigFileNames(allConfigFiles);
            var sqlMapConfigsWithDifferentName = otherConfigs.Where(e => new XmlParser(e.FilePath, e.ProjectName).Load().GetXmlNamespace() == BatisXmlFileTypeEnum.SqlMapConfig);

            if (sqlMapConfigsWithDifferentName.Any())
            {
                return sqlMapConfigsWithDifferentName.Distinct().ToList();
            }

            return new List<XmlFileInfo>();
        }

        public static List<XmlFileInfo> FindConfigsWithKnowFileName(IEnumerable<XmlFileInfo> allConfigFiles)
        {
            return allConfigFiles.Where(e => e.FilePath.ToLower().Contains(XmlConfigConstants.KnowFileName)).ToList();
        }

        public static List<XmlFileInfo> FilterDotNetCommonConfigFileNames(IEnumerable<XmlFileInfo> allConfigFiles)
        {
            return allConfigFiles.Where(e => !DotNetCommonConfigNames.Any(x => e.FilePath.ToLower().Contains(x.ToLower()))).ToList();
        }


    }
}
