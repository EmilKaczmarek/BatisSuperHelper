using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.Indexers.Models;

namespace VSIXProject5.Parsers
{
    public class XmlParser
    {
        private Lazy<string> _xmlNamespace => new Lazy<string>(GetDocumentXmlNamespace);
        public string XmlNamespace => _xmlNamespace.Value;

        private Lazy<string> _mapNamespace => new Lazy<string>(GetDocumentMapNamespace);
        public string MapNamespace =>_mapNamespace.Value;

        private Lazy<bool> _useStatementNamespaces => new Lazy<bool>(IsFileUsingStatementNamespaces);
        public bool IsUsingStatementNamespaces => _useStatementNamespaces.Value;

        private HtmlDocument _xmlDocument;
        private string _filePath;
        private string _fileName;
        private string _fileProjectName;


        private XmlParser()
        {
            _xmlDocument = new HtmlDocument();
        }

        private XmlParser(string filePath, string fileProject) :this()
        {           
            _filePath = filePath;
            _fileName = Path.GetFileName(filePath);
            _fileProjectName = fileProject;
        }

        public static async Task<XmlParser> WithFilePathAndFileInfoAsync(string filePath, string fileProjectName)
        {
            var instance = new XmlParser(filePath, fileProjectName);
            await Task.Run(() => instance._xmlDocument.Load(filePath));
            return instance;
        }

        public static XmlParser WithStringReaderAndFileInfo(StringReader stringReader, string filePath, string fileProjectName)
        {
            var instance = new XmlParser(filePath, fileProjectName);
            instance._xmlDocument.Load(stringReader);
            return instance;
        }

        public static XmlParser WithStringReader(StringReader stringReader)
        {
            var instance = new XmlParser();
            instance._xmlDocument.Load(stringReader);
            return instance;
        }

        public static XmlParser WithFilePathAndFileInfo(string filePath, string fileProjectName)
        {
            var instance = new XmlParser(filePath, fileProjectName);
            instance._xmlDocument.Load(filePath);
            return instance;
        }

        public static XmlParser WithFilePath(string filePath)
        {
            var instance = new XmlParser();
            instance._xmlDocument.Load(filePath);
            return instance;
        }

        public List<XmlIndexerResult> GetMapFileStatments()
        {
            var statementChildNodes = GetChildNodesOfParentByXPath(IBatisConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => IBatisHelper.IsIBatisStatment(e.Name)).Select(e => new XmlIndexerResult
            {
                QueryFileName = _fileName,
                QueryFilePath = _filePath,
                QueryId = IsUsingStatementNamespaces ? MapNamespaceHelper.CreateFullQueryString(MapNamespace, e.Id) : e.Id,
                QueryLineNumber = e.Line,
                QueryVsProjectName = _fileProjectName,
                MapNamespace = MapNamespace
        }).ToList();
        }

        public string GetQueryAtLineOrNull(int lineNumber)
        {
            var nodes = _xmlDocument.DocumentNode.Descendants();
            var lineNode = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

            return IsUsingStatementNamespaces? MapNamespaceHelper.CreateFullQueryString(MapNamespace, lineNode?.Id):lineNode?.Id;
        }

        public List<int> GetStatmentElementsLineNumber()
        {
            var statementChildNodes = GetChildNodesOfParentByXPath(IBatisConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => e.Name != "#text").Select(e => e.Line).ToList();
        }

        public bool HasSelectedLineValidQuery(int lineNumber)
        {
            var nodes = _xmlDocument.DocumentNode.Descendants();
            var line = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

            return line?.Name != null && IBatisConstants.StatementNames.Contains(line.Name);          
        }

        private HtmlNode GetFirstStatmentNodeForLineOrNull(IEnumerable<HtmlNode> nodes, int lineNumber)
        {
            nodes = nodes.Where(e => e.NodeType != HtmlNodeType.Text);
            var line = nodes.FirstOrDefault(e => e.Line == lineNumber) ?? nodes.FirstOrDefault(e => e.Line == nodes.Select(x => x.Line).DetermineClosestInt(lineNumber));

            while (line != null)
            {
                if (IBatisConstants.StatementNames.Contains(line.Name))
                {
                    return line;
                }
                line = line.ParentNode;
            }

            return null;
        }

        private IEnumerable<HtmlNode> GetChildNodesOfParentByXPath(string xPath)
        {
            var statementRootNode = _xmlDocument.DocumentNode.SelectSingleNode(xPath);

            if (statementRootNode == null)
            {
                return Enumerable.Empty<HtmlNode>();
            }
            return statementRootNode.Descendants();
        }

        private string GetDocumentXmlNamespace()
        {
            var fileRootNode = GetMapDocumentRootNode();
            return fileRootNode.Attributes.FirstOrDefault(e => e.Name == "xmlns").Value;
        }

        private string GetDocumentMapNamespace()
        {
            var fileRootNode = GetMapDocumentRootNode();
            var names = fileRootNode.Attributes.FirstOrDefault(e => e.Name == "namespace")?.Value;
            return names;
        }

        private HtmlNode GetMapDocumentRootNode()
        {
            return _xmlDocument.DocumentNode.SelectSingleNode(IBatisConstants.MapFileRootElementXPath);
        }

        private bool IsFileUsingStatementNamespaces()
        {
            var useStatmentNamespaceNodes = GetChildNodesOfParentByXPath(IBatisConstants.SettingRootElementXPath)?.FirstOrDefault(e => e.Name != "#text");
            
            if (useStatmentNamespaceNodes != null)
            {
                return useStatmentNamespaceNodes.GetAttributeValue(IBatisConstants.UseStatmentNameSpaceSettingAttributeName, false);
            }
         
            return false;
        }
    }
}
