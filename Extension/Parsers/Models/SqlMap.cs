using BatisSuperHelper.Parsers.Models.Shared;
using BatisSuperHelper.Parsers.Models.SqlMap;
using System.Collections.Generic;

namespace BatisSuperHelper.Parsers.Models
{
    public class SqlMapModel
    {
        public IEnumerable<TypeAlias> Alias { get; set; }
        //public IEnumerable<CacheModel> CacheModels { get; set; }//Not needed...
        public IEnumerable<ResultMap> ResultMaps { get; set; }
        public IEnumerable<Statement> Statements { get; set; }
        public IEnumerable<ParameterMap> ParametersMap { get; set; }
    }
}
