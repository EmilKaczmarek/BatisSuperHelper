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

namespace VSIXProject5.Indexers
{
    public class XmlIndexer
    {
        private string _solutionDir;
        public XmlIndexer() { }
        public XmlIndexer(string solutionDir)
        {
            _solutionDir = solutionDir;
        }

        private List<XmlIndexerResult> BuildUsingReader(XmlTextReader reader, string filePath)
        {
            using (reader)
            {
                var batch = new List<XmlIndexerResult>();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && IBatisHelper.IsIBatisStatment(reader.Name))
                    {
                        batch.Add(new XmlIndexerResult
                        {
                            QueryId = reader.GetAttribute(IBatisConstants.StatmentIdAttributeName),
                            QueryLineNumber = reader.LineNumber,
                            QueryFileName = Path.GetFileName(filePath),
                            QueryFilePath = filePath,
                        });
                    }
                }
                return batch;
            }
        }
        public List<XmlIndexerResult> BuildUsingFilePath(string filePath)
        {
            using (var reader = new XmlTextReader(filePath))
            {
                return BuildUsingReader(reader, filePath);             
            }
        }
        public List<XmlIndexerResult> BuildFromXDocString(string fileContent, string filePath)
        {
            using (var reader = new XmlTextReader(new StringReader(fileContent))){
                return BuildUsingReader(reader, filePath);
            }
        }
        public List<XmlIndexerResult> BuildIndexer(List<XmlFileInfo> solutionXmlDocuments)
        {
            var result = new List<XmlIndexerResult>();
            List<XmlFileInfo> sqlMapsFilesCollection = new List<XmlFileInfo>();
            foreach (var xmlSolutionDocument in solutionXmlDocuments)
            {
                XDocument xdoc = null;
                try
                {
                    xdoc = XDocument.Load(xmlSolutionDocument.FilePath);
                }
                catch (Exception)
                {
                    continue;
                }

                bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xdoc) == @"http://ibatis.apache.org/mapping";
                if (isIBatisQueryXmlFile)
                {
                    sqlMapsFilesCollection.Add(xmlSolutionDocument);
                }
            }
            return sqlMapsFilesCollection.Select(e =>
            {
                using (XmlTextReader reader = new XmlTextReader(e.FilePath))
                {
                    return BuildUsingReader(reader, e.FilePath);  
                }
            }).SelectMany(e => e).ToList();
        }
    }
}
