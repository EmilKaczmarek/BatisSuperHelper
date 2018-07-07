using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Models;
using VSIXProject5.Parsers;

namespace VSIXProject5.Indexers
{
    public class XmlIndexer:BaseIndexer
    {
        public List<XmlIndexerResult> BuildIndexerAsync(List<XmlFileInfo> solutionXmlDocuments)
        {
            var result = new List<XmlIndexerResult>();
            foreach (var xmlSolutionDocument in solutionXmlDocuments)
            {
                XmlParser parser = XmlParser.WithFilePathAndFileInfo(xmlSolutionDocument.FilePath, xmlSolutionDocument.ProjectName);

                bool isIBatisQueryXmlFile = parser.XmlNamespace == @"http://ibatis.apache.org/mapping";
                if (isIBatisQueryXmlFile)
                {
                    result.AddRange(parser.GetMapFileStatments());
                }
            }
            return result;
        }
    }
}
