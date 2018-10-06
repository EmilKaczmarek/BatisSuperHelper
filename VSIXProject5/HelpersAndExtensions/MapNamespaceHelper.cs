using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions
{
    public static class MapNamespaceHelper
    {
        public static Tuple<string,string> DetermineMapNamespaceQueryPairFromCodeInput(string queryWithNamespace)
        {
            var splited = queryWithNamespace.Split('.');
            if(splited.Count() < 2)
            {
                return null;
            }

            var query = splited.Last();
            return Tuple.Create(string.Join(".", splited.Take(splited.Length - 1)), query);
        }

        public static string CreateFullQueryString(string mapNamespace, string queryName)
        {
            return string.IsNullOrEmpty(mapNamespace)?queryName:$"{mapNamespace}.{queryName}";
        }
    }
}
