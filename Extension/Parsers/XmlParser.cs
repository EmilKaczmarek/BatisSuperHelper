using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IBatisSuperHelper.Parsers
{
    public class XmlParser
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string FileProjectName { get; private set; }

        private StringReader _stringReader;
        private HtmlDocument _xmlDocument;

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

        public void Load()
        {
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

    }
}
