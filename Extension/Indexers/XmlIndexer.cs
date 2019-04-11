using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers;

namespace IBatisSuperHelper.Indexers
{
    public class XmlIndexer
    {
        public List<XmlQuery> BuildIndexer(List<XmlFileInfo> solutionXmlDocuments)
        {
            var result = new List<XmlQuery>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var xmlSolutionDocument in solutionXmlDocuments)
            {
                XmlParser parser = new XmlParser().WithFileInfo(xmlSolutionDocument.FilePath, xmlSolutionDocument.ProjectName).Load();

                bool isIBatisQueryXmlFile = parser.XmlNamespace == @"http://ibatis.apache.org/mapping";
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
            XmlParser parser = new XmlParser().WithFileInfo(xmlDocument.FilePath, xmlDocument.ProjectName).Load();

            bool isIBatisQueryXmlFile = parser.XmlNamespace == @"http://ibatis.apache.org/mapping";
            if (isIBatisQueryXmlFile)
            {
                return parser.GetMapFileStatments();
            }
            return new List<XmlQuery>();
        }

    }
}
