﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.Models.Shared;
using IBatisSuperHelper.Parsers.Models.SqlMap;

namespace IBatisSuperHelper.Parsers
{
    public class BatisXmlMapParser : XmlParser
    {
        public SqlMapModel Result { get; private set; }
        
        private Lazy<string> _xmlNamespace => new Lazy<string>(GetDocumentXmlNamespace);
        public new string XmlNamespace => _xmlNamespace.Value;

        private Lazy<string> _mapNamespace => new Lazy<string>(GetDocumentMapNamespace);
        public string MapNamespace =>_mapNamespace.Value;
    
        #region Construction
        public BatisXmlMapParser()
        {
            InitializeEmpty();
        }

        public BatisXmlMapParser(XmlParser parser) : base(parser)
        {

        }

        public BatisXmlMapParser WithStringReader(StringReader stringReader)
        {
            InitializeWithStringReader(stringReader);
            return this;
        }

        public BatisXmlMapParser WithFileInfo(string filePath, string fileProjectName)
        {
            InitializeWithFilePathAndProjectName(filePath, fileProjectName);
            return this;
        }

        public new BatisXmlMapParser Load()
        {
            base.Load();
            return this;
        }
        #endregion


        public List<Statement> GetMapFileStatments()
        {
            var statementChildNodes = GetChildNodesOfParentByXPath(XmlMapConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => IBatisHelper.IsIBatisStatment(e.Name)).Select(e => new Statement
            {
                QueryFileName = FileName,
                QueryFilePath = FilePath,
                QueryId = e.Id,
                FullyQualifiedQuery = MapNamespaceHelper.CreateFullQueryString(MapNamespace, e.Id),
                QueryLineNumber = e.Line,
                QueryVsProjectName = FileProjectName,
                MapNamespace = MapNamespace,
                CacheModel = e.GetAttributeValue("cacheModel", null),
                ListClass = e.GetAttributeValue("listClass", null),
                ParameterClass = e.GetAttributeValue("parameterClass", null),
                ParameterMap = e.GetAttributeValue("parameterMap", null),
                ResultClass = e.GetAttributeValue("resultClass", null),
                ResultMap = e.GetAttributeValue("resultMap", null),
                //Type = e.GetAttributeValue("type", null),
                Content = e.InnerHtml,

            }).ToList();
        }

        public List<Statement> GetMapFileStatmentsWithIdAttributeColumnInfo()
        {
            var statementChildNodes = GetChildNodesOfParentByXPath(XmlMapConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => IBatisHelper.IsIBatisStatment(e.Name)).Select(e => new Statement
            {
                XmlLine = e.Attributes.FirstOrDefault(x => x.Name == "id")?.Line,
                XmlLineColumn = e.Attributes.FirstOrDefault(x => x.Name == "id")?.LinePosition,
                QueryFileName = FileName,
                QueryFilePath = FilePath,
                QueryId = e.Id,
                FullyQualifiedQuery = MapNamespaceHelper.CreateFullQueryString(MapNamespace, e.Id),
                QueryLineNumber = e.Line,
                QueryVsProjectName = FileProjectName,
                MapNamespace = MapNamespace,
                CacheModel = e.GetAttributeValue("cacheModel", null),
                ListClass = e.GetAttributeValue("listClass", null),
                ParameterClass = e.GetAttributeValue("parameterClass", null),
                ParameterMap = e.GetAttributeValue("parameterMap", null),
                ResultClass = e.GetAttributeValue("resultClass", null),
                ResultMap = e.GetAttributeValue("resultMap", null),
                //Type = e.GetAttributeValue("type", null),
                Content = e.InnerHtml,
            }).ToList();
        }

        public string GetQueryAtLineOrNull(int lineNumber, bool useNamespace)
        {
            var nodes = GetAllDescendantsNodes();
            var lineNode = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

            if (!useNamespace)
            {
                return lineNode?.Id;
            }

            return MapNamespaceHelper.CreateFullQueryString(MapNamespace, lineNode?.Id);
        }

        public List<int> GetStatmentElementsLineNumber()
        {
            var statementChildNodes = GetChildNodesOfParentByXPath(XmlMapConstants.StatementsRootElementXPath);
            return statementChildNodes.Where(e => e.Name != "#text").Select(e => e.Line).ToList();
        }

        public bool HasSelectedLineValidQuery(int lineNumber)
        {
            var nodes = GetAllDescendantsNodes();
            var line = GetFirstStatmentNodeForLineOrNull(nodes, lineNumber);

            return line?.Name != null && XmlMapConstants.StatementNames.Contains(line.Name);          
        }

        private HtmlNode GetFirstStatmentNodeForLineOrNull(IEnumerable<HtmlNode> nodes, int lineNumber)
        {
            nodes = nodes.Where(e => e.NodeType != HtmlNodeType.Text);
            var line = nodes.FirstOrDefault(e => e.Line == lineNumber) ?? nodes.FirstOrDefault(e => e.Line == nodes.Select(x => x.Line).DetermineClosestInt(lineNumber));

            while (line != null)
            {
                if (XmlMapConstants.StatementNames.Contains(line.Name))
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
            return GetSingleNode(XmlMapConstants.MapFileRootElementXPath);
        }

    }
}
