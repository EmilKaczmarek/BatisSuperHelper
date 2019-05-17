using IBatisSuperHelper.Parsers.Models.Shared;
using IBatisSuperHelper.Parsers.Models.SqlMap;
using System.Collections.Generic;

namespace IBatisSuperHelper.Parsers.Models
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
