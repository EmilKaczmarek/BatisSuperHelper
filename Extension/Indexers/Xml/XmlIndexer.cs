using System.Collections.Generic;
using System.Diagnostics;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;

namespace IBatisSuperHelper.Indexers.Xml
{
    public class XmlIndexer
    {
        public List<XmlQuery> BuildIndexer(IEnumerable<XmlFileInfo> solutionXmlDocuments)
        {
            var result = new List<XmlQuery>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var xmlSolutionDocument in solutionXmlDocuments)
            {
                BatisXmlMapParser parser = new BatisXmlMapParser().WithFileInfo(xmlSolutionDocument.FilePath, xmlSolutionDocument.ProjectName).Load();

                bool isIBatisQueryXmlFile = parser.XmlNamespace == XmlMapConstants.XmlNamespace;
                if (isIBatisQueryXmlFile)
                {
                    result.AddRange(parser.GetMapFileStatments());
                }
            }
            sw.Stop();
            OutputWindowLogger.WriteLn($"Building Queries db from xml ended in {sw.ElapsedMilliseconds} ms. Found {result.Count} queries.");
            return result;
        }

        public List<XmlQuery> ParseSingleFile(XmlFileInfo xmlDocument)
        {
            BatisXmlMapParser parser = new BatisXmlMapParser().WithFileInfo(xmlDocument.FilePath, xmlDocument.ProjectName).Load();

            bool isIBatisQueryXmlFile = parser.XmlNamespace == XmlMapConstants.XmlNamespace;
            if (isIBatisQueryXmlFile)
            {
                return parser.GetMapFileStatments();
            }
            return new List<XmlQuery>();
        }

        public SqlMapConfig ParseSingleConfigFile(XmlFileInfo xmlDocument)
        {
            BatisXmlConfigParser parser = new BatisXmlConfigParser().WithFileInfo(xmlDocument.FilePath, xmlDocument.ProjectName).Load();

            if (parser.XmlNamespace == XmlConfigConstants.XmlNamespace)
            {
                return parser.Result;
            }

            return new SqlMapConfig {ParsedSuccessfully = false};

        }

    }
}
