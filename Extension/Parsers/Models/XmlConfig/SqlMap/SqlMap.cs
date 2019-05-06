using IBatisSuperHelper.Constants.BatisConstants;
using System.Collections.Generic;
using System.Linq;
using static IBatisSuperHelper.Constants.BatisConstants.XmlConfigConstants;

namespace IBatisSuperHelper.Parsers.Models.XmlConfig.SqlMap
{
    public class SqlMap
    {
        public string RawValue { get; set; }
        public string FileName { get; set; }
        public SqlMapResourceType ResourceType { get; set; }

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

        public static SqlMap FromAttribute(string attributeName, string attribueValue)
        {
            switch (attributeName)
            {
                case ResourceAttributeName:
                    return new SqlMap
                    {
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.RESOURCE,
                    };
                case EmbeddedAttributeName:
                    return new SqlMap
                    {
                        FileName = GetFileNameFromEmbeddedRawValue(attribueValue),
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.EMBEDDED,
                    };
                case UriAttributeName:
                    return new SqlMap
                    {
                        RawValue = attribueValue,
                        ResourceType = SqlMapResourceType.URI,
                    };
                default:
                    return new SqlMap
                    {
                        ResourceType = SqlMapResourceType.UNKNOWN,
                    };
            }
        }

        public override bool Equals(object obj)
        {
            var map = obj as SqlMap;
            return map != null &&
                   RawValue == map.RawValue &&
                   FileName == map.FileName &&
                   ResourceType == map.ResourceType;
        }

        public override int GetHashCode()
        {
            var hashCode = -814287421;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RawValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + ResourceType.GetHashCode();
            return hashCode;
        }
    }
}
