using System.Collections.Generic;

namespace VSIXProject5.Indexers
{
    public class IndexerKey
    {
        public string StatmentName { get; set; }
        public string VsProjectName { get; set; }
        public string MapNamespace { get; set; }

        public static IndexerKey ConvertToKey(string statmentName, string vsProjectName)
        {
            return new IndexerKey {
                StatmentName = statmentName,
                VsProjectName= vsProjectName,
            };
        }

        public static IndexerKey ConvertToKey(string statmentName, string vsProjectName, string mapNamespace)
        {
            return new IndexerKey
            {
                StatmentName = statmentName,
                VsProjectName = vsProjectName,
                MapNamespace = mapNamespace,
            };
        }
    }

}
