using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Models;

namespace IBatisSuperHelper.Parsers
{
    public class BatisXmlMapParser
    {
        private Lazy<string> _xmlNamespace => new Lazy<string>(GetDocumentXmlNamespace);
        public string XmlNamespace => _xmlNamespace.Value;

        private Lazy<string> _mapNamespace => new Lazy<string>(GetDocumentMapNamespace);
        public string MapNamespace =>_mapNamespace.Value;

        private Lazy<bool> _useStatementNamespaces => new Lazy<bool>(IsFileUsingStatementNamespaces);
        public bool IsUsingStatementNamespaces => _useStatementNamespaces.Value;

        private XmlParser _parser;

        public BatisXmlMapParser()
        {
            _parser = new XmlParser();
        }

        public BatisXmlMapParser WithStringReader(StringReader stringReader)
        {
            _parser = new XmlParser(stringReader);
            return this;
        }

        public BatisXmlMapParser WithFileInfo(string filePath, string fileProjectName)
        {
            _parser = new XmlParser(filePath, fileProjectName);
            return this;
        }

        public new BatisXmlMapParser Load()
        {
            _parser.Load();
            return this;
        }

        public List<XmlQuery> GetMapFileStatments()
        {
            var statementChildNodes = _parser.GetChildNodesOfParentByXPath(IBatisConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => IBatisHelper.IsIBatisStatment(e.Name)).Select(e => new XmlQuery
            {
                QueryFileName = _parser.FileName,
                QueryFilePath = _parser.FilePath,
                QueryId = e.Id,
                FullyQualifiedQuery = IsUsingStatementNamespaces ? MapNamespaceHelper.CreateFullQueryString(MapNamespace, e.Id) : e.Id,
                QueryLineNumber = e.Line,
                QueryVsProjectName = _parser.FileProjectName,
                MapNamespace = MapNamespace
            }).ToList();
        }

        public List<XmlQuery> GetMapFileStatmentsWithIdAttributeColumnInfo()
        {
            var statementChildNodes = _parser.GetChildNodesOfParentByXPath(IBatisConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => IBatisHelper.IsIBatisStatment(e.Name)).Select(e => new XmlQuery
            {
                XmlLine = e.Attributes.FirstOrDefault(x => x.Name == "id")?.Line,
                XmlLineColumn = e.Attributes.FirstOrDefault(x=>x.Name == "id")?.LinePosition,
                QueryFileName = _parser.FileName,
                QueryFilePath = _parser.FilePath,
                QueryId = e.Id,
                FullyQualifiedQuery = IsUsingStatementNamespaces ? MapNamespaceHelper.CreateFullQueryString(MapNamespace, e.Id) : e.Id,
                QueryLineNumber = e.Line,
                QueryVsProjectName = _parser.FileProjectName,
                MapNamespace = MapNamespace
            }).ToList();
        }

        public string GetQueryAtLineOrNull(int lineNumber, bool forceNoNamespace)
        {
            if (forceNoNamespace)
            {
                var nodes = _parser.GetAllDescendantsNodes();
                var lineNode = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

                return lineNode?.Id;
            }

            return GetQueryAtLineOrNull(lineNumber);
        }

        public string GetQueryAtLineOrNull(int lineNumber)
        {
            var nodes = _parser.GetAllDescendantsNodes();
            var lineNode = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

            return IsUsingStatementNamespaces? MapNamespaceHelper.CreateFullQueryString(MapNamespace, lineNode?.Id):lineNode?.Id;
        }

        public List<int> GetStatmentElementsLineNumber()
        {
            var statementChildNodes = _parser.GetChildNodesOfParentByXPath(IBatisConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => e.Name != "#text").Select(e => e.Line).ToList();
        }

        public bool HasSelectedLineValidQuery(int lineNumber)
        {
            var nodes = _parser.GetAllDescendantsNodes();
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

        private string GetDocumentXmlNamespace()
        {
            var fileRootNode = GetMapDocumentRootNode();
            if (fileRootNode == null)
                return null;

            return fileRootNode.Attributes.FirstOrDefault(e => e.Name == "xmlns")?.Value;
        }

        private string GetDocumentMapNamespace()
        {
            var fileRootNode = GetMapDocumentRootNode();
            if (fileRootNode == null)
                return null;

            return fileRootNode.Attributes.FirstOrDefault(e => e.Name == "namespace")?.Value;
        }

        private HtmlNode GetMapDocumentRootNode()
        {
            return _parser.GetSingleNode(IBatisConstants.MapFileRootElementXPath);
        }

        private bool IsFileUsingStatementNamespaces()
        {
            var useStatmentNamespaceNodes = _parser.GetChildNodesOfParentByXPath(IBatisConstants.SettingRootElementXPath)?.FirstOrDefault(e => e.Name != "#text");
            
            if (useStatmentNamespaceNodes != null)
            {
                return useStatmentNamespaceNodes.GetAttributeValue(IBatisConstants.UseStatmentNameSpaceSettingAttributeName, false);
            }
         
            return false;
        }
    }
}
