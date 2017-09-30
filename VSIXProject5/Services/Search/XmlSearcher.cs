using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace iBatisSuperHelper.Services.Search
{
    public class XmlSearcher : IFileSearcher
    {
        public SearchResult Execute(string searchString, string directory)
        {
            List<SearchResult> ret = new List<SearchResult>();
            var files = Directory.EnumerateFiles(directory);
            var searchResult = new SearchResult
            {
                FileDirectory = directory,
                SearchLocations = SearchInFiles(searchString, files.ToList()),
            };
          
            return searchResult;
        }
        public List<SearchLocation> SearchInFiles(string searchString,List<string> files)
        {
            var searchLocations = new List<SearchLocation>();
            foreach (var file in files)
            {
                string[] lines = File.ReadAllLines(file);
                var xdoc = XDocument.Parse(string.Join("\n", lines), LoadOptions.SetLineInfo);
                var node = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "statements");
                var statments2 = node.Descendants();
                var s = statments2.AsParallel().FirstOrDefault(e => e.FirstAttribute.Value == searchString);
                var t = ((IXmlLineInfo)s);
                if (s != null)
                {
                    var lineNumer = t.LineNumber - 1;
                    int startingIndex = lines[lineNumer].IndexOf(searchString);
                    int endIndex = lines[lineNumer].IndexOf("\"", startingIndex);
                    string testSubstring = lines[lineNumer].Substring(startingIndex, searchString.Length);
                    searchLocations.Add(new SearchLocation
                    {
                        LineNumber = lineNumer,
                        StartIndex = startingIndex,
                    });
                }
            }
            return searchLocations;
        }
        public List<SearchResult> SearchInFiles(string searchString, List<DocumentProperties> documentProperties)
        {
            var searchLocations = new List<SearchLocation>();
            var searchResults = new List<SearchResult>();
            foreach (var property in documentProperties)
            {
                var file = property.FilePath;
                string[] lines = File.ReadAllLines(file);
                XDocument xdoc = null;//XDocument.Parse(string.Join("\n", lines), LoadOptions.SetLineInfo);
                try
                {
                    xdoc = XDocument.Parse(string.Join("\n", lines), LoadOptions.SetLineInfo);
                }
                catch (Exception)
                {
                    continue;
                }
                var node = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "statements");
                if (node == null) continue;
                var statments2 = node.Descendants();
                var s = statments2.AsParallel().FirstOrDefault(e => e.FirstAttribute.Value == searchString);
                var t = ((IXmlLineInfo)s);
                if (s != null)
                {
                    var lineNumer = t.LineNumber-1;
                    int startingIndex = lines[lineNumer].IndexOf(searchString);
                    int endIndex = lines[lineNumer].IndexOf("\"", startingIndex);
                    string testSubstring = lines[lineNumer].Substring(startingIndex, searchString.Length);
                    //searchLocations.Add(new SearchLocation
                    //{
                    //    LineNumber = lineNumer+1,
                    //    StartIndex = startingIndex,
                    //});
                    searchResults.Add(new SearchResult
                    {
                        RelativeVsPath = property.RelativePath,
                        SearchLocations = new List<SearchLocation>
                        {
                            new SearchLocation
                            {
                                LineNumber = lineNumer+1,
                                StartIndex = startingIndex,
                            }
                        }
                    });
                }
            }
            return searchResults;
        }
    }
}
