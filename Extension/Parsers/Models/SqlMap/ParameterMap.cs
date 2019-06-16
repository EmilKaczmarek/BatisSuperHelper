using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Parsers.Models.SqlMap
{
    public class ParameterMap
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public string Extends { get; set; }
        public IEnumerable<Parameter> Parameters {get;set;}
    }
}
