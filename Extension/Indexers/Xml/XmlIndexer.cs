using System.Collections.Generic;
using System.Diagnostics;
using BatisSuperHelper.Constants.BatisConstants;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Models;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Parsers.Models.SqlMap;

namespace BatisSuperHelper.Indexers.Xml
{
    public class XmlIndexer
    {
        public List<Statement> BuildIndexer(IDictionary<SqlMapConfig, IEnumerable<XmlFileInfo>> configFileInfosPairs)
        {
            var result = new List<Statement>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var configFilesPair in configFileInfosPairs)
            {
                foreach (var xmlSolutionDocument in configFilesPair.Value)
                {
                    BatisXmlMapParser parser = new BatisXmlMapParser().WithFileInfo(xmlSolutionDocument.FilePath, xmlSolutionDocument.ProjectName).Load();

                    bool isBatisQueryXmlFile = parser.XmlNamespace == XmlMapConstants.XmlNamespace;
                    if (isBatisQueryXmlFile)
                    {
                        result.AddRange(parser.GetMapFileStatments());
                    }
                }
            }
            sw.Stop();
            OutputWindowLogger.WriteLn($"Building Queries db from xml ended in {sw.ElapsedMilliseconds} ms. Found {result.Count} queries.");
            return result;
        }

        public List<Statement> ParseSingleFile(XmlFileInfo xmlDocument)
        {
            BatisXmlMapParser parser = new BatisXmlMapParser().WithFileInfo(xmlDocument.FilePath, xmlDocument.ProjectName).Load();

            bool isBatisQueryXmlFile = parser.XmlNamespace == XmlMapConstants.XmlNamespace;
            if (isBatisQueryXmlFile)
            {
                return parser.GetMapFileStatments();
            }
            return new List<Statement>();
        }

        public SqlMapConfig ParseSingleConfigFile(XmlFileInfo xmlDocument)
        {
            BatisXmlConfigParser parser = new BatisXmlConfigParser().WithFileInfo(xmlDocument.FilePath, xmlDocument.ProjectName).Load();

            if (parser.XmlNamespace == Constants.BatisConstants.XmlConfigConstants.XmlNamespace)
            {
                return parser.Result;
            }

            return new SqlMapConfig {ParsedSuccessfully = false};

        }

    }
}
