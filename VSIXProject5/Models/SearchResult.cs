using System.Collections.Generic;

namespace iBatisSuperHelper.Services.Search
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
        public int SearchDataLength => SearchData.Length == 0 ? -1 : SearchData.Length;
    }
}
