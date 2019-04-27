using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IBatisSuperHelper.Parsers
{
    public class XmlParser
    {
        public string XmlNamespace => IsLazy ? _xmlNamespaceLazy.Value : GetXmlNamespace();
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string FileProjectName { get; private set; }

        private StringReader _stringReader;
        private HtmlDocument _xmlDocument;

        private Lazy<string> _xmlNamespaceLazy;
        protected bool IsLazy = false;//I am...
        
        public XmlParser()
        {
            InitializeEmpty();
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
            _xmlNamespaceLazy = new Lazy<string>(() => GetXmlNamespace());
        }

        public void Load()
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
        }

        public IEnumerable<HtmlNode> GetChildNodesOfParentByXPath(string xPath)
        {
            var test = _xmlDocument.DocumentNode.DescendantNodes();//TODO: Remove after tests.
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

        public string GetXmlNamespace()
        {
            return _xmlDocument.DocumentNode.ChildNodes.Count != 2 ? null :_xmlDocument.DocumentNode.ChildNodes.Last().GetAttributeValue("xmlns", null);
        }

    }
}
