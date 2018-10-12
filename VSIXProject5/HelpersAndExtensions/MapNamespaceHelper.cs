using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers.Models;

namespace VSIXProject5.HelpersAndExtensions
{
    public static class MapNamespaceHelper
    {
        public static Tuple<string,string> DetermineMapNamespaceQueryPairFromCodeInput(string queryWithNamespace)
        {
            if (queryWithNamespace == null)
                return null;

            var splited = queryWithNamespace.Split('.');
            if(splited.Count() < 2)
            {
                return Tuple.Create(String.Empty, queryWithNamespace);
            }

            var query = splited.Last();
            return Tuple.Create(string.Join(".", splited.Take(splited.Length - 1)), query);
        }

        public static string CreateFullQueryString(string mapNamespace, string queryName)
        {
            return queryName == null? null : (string.IsNullOrEmpty(mapNamespace)?queryName:$"{mapNamespace}.{queryName}");
        }

        public static string GetQueryWithoutNamespace(string queryWithNamespace)
        {
            return DetermineMapNamespaceQueryPairFromCodeInput(queryWithNamespace).Item2;
        }

        public static string GetQueryWithoutNamespace(XmlIndexerResult xmlIndexerResult)
        {
            if(xmlIndexerResult != null && xmlIndexerResult.MapNamespace != null)
            {
                return xmlIndexerResult.QueryId.Replace($"{xmlIndexerResult.MapNamespace}.", "");
            }
            return xmlIndexerResult.QueryId;
        }
    }
}
