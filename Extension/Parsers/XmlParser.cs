using BatisSuperHelper.Constants.BatisConstants;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatisSuperHelper.Parsers
{
    public class XmlParser
    {
        public BatisXmlFileTypeEnum BatisXmlFileType => IsLazy ? _xmlNamespaceLazy.Value : GetXmlNamespace();
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string FileProjectName { get; private set; }

        private StringReader _stringReader;
        internal HtmlDocument _xmlDocument;

        private Lazy<BatisXmlFileTypeEnum> _xmlNamespaceLazy;
        protected bool IsLazy = false;//I am...
        
        public XmlParser()
        {
            InitializeEmpty();
        }

        public XmlParser(XmlParser otherInstance)
        {
            FilePath = otherInstance.FilePath;
            FileName = otherInstance.FileName;
            FileProjectName = otherInstance.FileProjectName;
            _stringReader = otherInstance._stringReader;
            _xmlDocument = otherInstance._xmlDocument;
            IsLazy = otherInstance.IsLazy;
        }

        public XmlParser(StringReader stringReader)
        {
            InitializeWithStringReader(stringReader);
        }

        public XmlParser(string filePath, string fileProjectName)
        {
            InitializeWithFilePathAndProjectName(filePath, fileProjectName);
        }

        public void InitializeEmpty()
        {
            _xmlDocument = new HtmlDocument();
            _xmlDocument.OptionOutputAsXml = true;
        }

        public void InitializeWithStringReader(StringReader stringReader)
        {
            _stringReader = stringReader;
        }

        public void InitializeWithFilePathAndProjectName(string filePath, string fileProjectName)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileProjectName = fileProjectName;
        }

        public void LazyLoading()
        {
            IsLazy = true;
            _xmlNamespaceLazy = new Lazy<BatisXmlFileTypeEnum>(() => GetXmlNamespace());
        }

        public XmlParser Load()
        {
            InitializeEmpty();
            if (_stringReader != null)
            {
                _xmlDocument.Load(_stringReader);
            }
            else
            {
                _xmlDocument.Load(FilePath);
            }

            return this;
        }

        public HtmlDocument GetDocument()
        {
            return _xmlDocument;
        }

        public IEnumerable<HtmlNode> GetChildNodesOfParentByXPath(string xPath)
        {
            var statementRootNode = GetSingleNode(xPath);

            if (statementRootNode == null)
            {
                return Enumerable.Empty<HtmlNode>();
            }
            return statementRootNode.Descendants();
        }

        public IEnumerable<HtmlNode> GetAllDescendantsNodes()
        {
            return _xmlDocument.DocumentNode.Descendants();
        }

        public HtmlNode GetSingleNode(string xpath)
        {
            return _xmlDocument.DocumentNode.SelectSingleNode(xpath);
        }

        public BatisXmlFileTypeEnum GetXmlNamespace()
        {
            if (_xmlDocument.DocumentNode.ChildNodes.SingleOrDefault(e=>e.OriginalName == XmlMapConstants.RootElementName) != null)
            {
                return BatisXmlFileTypeEnum.SqlMap; 
            }

            if(_xmlDocument.DocumentNode.ChildNodes.SingleOrDefault(e=>e.OriginalName == XmlConfigConstants.RootElementName) != null)
            {
                return BatisXmlFileTypeEnum.SqlMapConfig; 
            }

            return BatisXmlFileTypeEnum.Other;
        }

    }
}
