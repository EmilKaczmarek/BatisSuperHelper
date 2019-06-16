using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Parsers.Models.SqlMap
{
    public class ResultMap
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public string Extends { get; set; }
        public IEnumerable<ResultProperties> Properties {get; set;}
    }
}
