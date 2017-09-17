using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Search;

namespace VSIXProject5.Searchers
{
    public class SearchResult
    {
        public string SearchData { get; set; }
        public string FileName { get; set; }
        public string FileDirectory { get; set; }
        public string RelativeVsPath { get; set; }
        public List<SearchLocation> SearchLocations { get; set; }
        public int LineNumber { get; set; }
        public int StartIndex { get; set; }
        public int SearchDataLength
        {
            get { return SearchData.Length == 0 ? -1 : SearchData.Length; }
        }
    }
}
