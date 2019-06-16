using BatisSuperHelper.Constants.BatisConstants;
using System.Collections.Generic;
using System.Linq;
using static BatisSuperHelper.Constants.BatisConstants.XmlConfigConstants;

namespace BatisSuperHelper.Parsers.Models.XmlConfig.SqlMap
{
    public class SqlMapDefinition
    {
        public string RawValue { get; set; }
        public string Value { get; set; }
        public Constants.BatisConstants.XmlConfigConstants.SqlMapResourceType ResourceType { get; set; }

        private static string GetFileNameFromEmbeddedRawValue(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }
            var splited = rawValue.Split(',');
            if (splited.Any() && splited.Length > 2)
            {
                return null;
            }
            return splited.First().Trim();
        }

        private static string GetFileNameFromAssembly(string assembly)
        {
            //Mynamespace.sqlMap.xml
            if (string.IsNullOrWhiteSpace(assembly))
            {
                return null;
            }
            var splited = assembly.Split('.');
            if (splited.Length < 2)
            {
                return null;
            }
            return $"{splited[splited.Length - 2]}.{splited[splited.Length - 1]}";
        }

        public static SqlMap FromAttribute(string attributeName, string attribueValue)
        {
            switch (attributeName)
            {
                case ResourceAttributeName:
                    return new SqlMapDefinition
                    {
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.RESOURCE,
                    };
                case EmbeddedAttributeName:
                    return new SqlMapDefinition
                    {
                        Value = GetFileNameFromAssembly(GetFileNameFromEmbeddedRawValue(attribueValue)),
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.EMBEDDED,
                    };
                case UriAttributeName:
                    return new SqlMapDefinition
                    {
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.URI,
                    };
                default:
                    return new SqlMapDefinition
                    {
                        ResourceType = SqlMapResourceType.UNKNOWN,
                    };
            }
        }

        public override bool Equals(object obj)
        {
            var map = obj as SqlMapDefinition;
            return map != null &&
                   RawValue == map.RawValue &&
                   Value == map.Value &&
                   ResourceType == map.ResourceType;
        }

        public override int GetHashCode()
        {
            var hashCode = -814287421;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RawValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + ResourceType.GetHashCode();
            return hashCode;
        }
    }
}
