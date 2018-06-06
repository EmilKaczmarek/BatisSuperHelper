using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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

        public List<XmlIndexerResult> BuildFromFile(string fileDirectory)
        {
            using(XmlTextReader reader = new XmlTextReader(fileDirectory))
            {
                var batch = new List<XmlIndexerResult>();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && IBatisHelper.IsIBatisStatment(reader.Name))
                    {
                        batch.Add(new XmlIndexerResult
                        {
                            QueryId = reader.GetAttribute("id"),
                            QueryLineNumber = reader.LineNumber,
                            QueryFileName = Path.GetFileName(fileDirectory),
                            QueryFilePath = fileDirectory,
                        });
                    }
                }
                return batch;
            } 
        }
        public List<XmlIndexerResult> BuildFromXDocString(string fileContent, string filePath)
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(fileContent)))
            {
                var batch = new List<XmlIndexerResult>();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && IBatisHelper.IsIBatisStatment(reader.Name))
                    {
                        batch.Add(new XmlIndexerResult
                        {
                            QueryId = reader.GetAttribute("id"),
                            QueryLineNumber = reader.LineNumber,
                            QueryFileName = Path.GetFileName(filePath),
                            QueryFilePath = filePath,
                        });
                    }
                }
                return batch;
            }
        }
        public List<XmlIndexerResult> BuildIndexer(List<SimpleProjectItem> solutionXmlDocuments)
        {
            var result = new List<XmlIndexerResult>();
            List<SimpleProjectItem> sqlMapsFilesCollection = new List<SimpleProjectItem>();
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
                    var batch = new List<XmlIndexerResult>();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (IBatisHelper.IsIBatisStatment(reader.Name))
                            {
                                batch.Add(new XmlIndexerResult
                                {
                                    QueryId = reader.GetAttribute("id"),
                                    QueryLineNumber = reader.LineNumber,
                                    QueryFileName = Path.GetFileName(e.FilePath),
                                    QueryVsProjectName = e.ProjectName,
                                    QueryFilePath = e.FilePath,
                                });
                            }
                        }
                    }
                    return batch;
                }
            }).SelectMany(e => e).ToList();
        }
        
        public List<XmlIndexerResult> BuildIndexer(string dir)
        {
            var result = new List<XmlIndexerResult>();
            string solutionDir = dir;
            var allXmlFilesInSolution = Directory.EnumerateFiles(solutionDir, "*.xml", SearchOption.AllDirectories);
            List<String> sqlMapsFilesCollection = new List<String>();
            foreach(var xmlFile in allXmlFilesInSolution)
            {
                XDocument xdoc = null;
                try
                {
                    xdoc = XDocument.Load(xmlFile);
                }
                catch (Exception)
                {
                    continue;
                }
                var nodes = xdoc.DescendantNodes();
                bool isIBatisQueryXmlFile = ((XElement)nodes.First()).Name.NamespaceName == @"http://ibatis.apache.org/mapping";
                if (isIBatisQueryXmlFile)
                {
                    sqlMapsFilesCollection.Add(xmlFile);
                }
            }
            return sqlMapsFilesCollection.Select(e =>
            {
                const string split = "\\";
                var splitted = dir.Split(new string[] { split }, StringSplitOptions.None);
                var splitted2 = e.Split(new string[] { split }, StringSplitOptions.None);
                string relativePath = string.Join("\\", splitted2.Skip(splitted.Count()-1));
                string test = $"{relativePath.Replace(@"\\", @"\")}";
                var batch = new List<XmlIndexerResult>();
                using(XmlTextReader reader = new XmlTextReader(e))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = reader.Name;
                            if (elementName == "select" ||
                               elementName == "insert" ||
                               elementName == "delete" ||
                               elementName == "update" ||
                               elementName == "statement" ||
                               elementName == "procedure" ||
                               elementName == "sql"
                               )
                            {
                                batch.Add(new XmlIndexerResult
                                {
                                    QueryId = reader.GetAttribute("id"),
                                    QueryLineNumber = reader.LineNumber,
                                    QueryFileName = Path.GetFileName(e),
                                    //QueryFileRelativeVsPath = $"{relativePath.Replace(@"\\", @"\")}",
                                    QueryFilePath = e,
                                });
                            }
                        }
                    }
                }               
                return batch;
            }).SelectMany(e => e).ToList();
        }
    }
}
