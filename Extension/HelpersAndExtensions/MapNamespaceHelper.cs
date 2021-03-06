﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Parsers.Models.SqlMap;

namespace BatisSuperHelper.HelpersAndExtensions
{
    public static class MapNamespaceHelper
    {
        public static bool IsQueryWithNamespace(string query)
        {
            return query.Split('.').Count() > 1;
        }

        public static Tuple<string,string> DetermineMapNamespaceQueryPairFromCodeInput(string queryWithNamespace)
        {
            if (queryWithNamespace == null)
               return Tuple.Create(string.Empty, string.Empty);

            var splited = queryWithNamespace.Split('.');
            if(splited.Count() < 2)
            {
                return Tuple.Create(string.Empty, queryWithNamespace);
            }

            var query = splited.Last();
            return Tuple.Create(string.Join(".", splited.Take(splited.Length - 1)), query);
        }

        public static string CreateFullQueryString(string mapNamespace, string queryName)
        {
            var query = string.IsNullOrEmpty(mapNamespace) ? queryName : $"{mapNamespace}.{queryName}";
            return string.IsNullOrEmpty(queryName) ? string.Empty : query;
        }

        public static string GetQueryWithoutNamespace(string queryWithNamespace)
        {
            return DetermineMapNamespaceQueryPairFromCodeInput(queryWithNamespace)?.Item2;
        }

        public static string GetQueryWithoutNamespace(Statement xmlIndexerResult)
        {
            if(xmlIndexerResult != null && xmlIndexerResult.MapNamespace != null)
            {
                return xmlIndexerResult.QueryId.Replace($"{xmlIndexerResult.MapNamespace}.", "");
            }
            return string.Empty;
        }
    }
}
